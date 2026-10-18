using System;
using UnityEngine;

/// <summary>
/// Q possesses the skeleton for the nearby locked door, and Q releases it.
/// One press cannot both start and end possession.
/// </summary>
[DefaultExecutionOrder(-50)]
public class PossessionController : MonoBehaviour
{
    [SerializeField] GameplayInput input;
    [SerializeField] ThirdPersonMotor witch;
    [SerializeField] ThirdPersonCamera followCamera;
    [SerializeField] Mana mana;
    [SerializeField] WitchLives lives;
    [SerializeField] LevelPuzzle[] puzzles;

    bool controllingSkeleton;
    bool gameOver;
    bool lifeLostForThisDepletion;
    LevelPuzzle activePuzzle;

    public bool ControllingSkeleton => controllingSkeleton;
    public bool IsGameOver => gameOver;
    public bool HasWon { get; private set; }
    public event Action Won;
    public string PromptText { get; private set; } = "";

    void Awake()
    {
        if (input == null || witch == null || followCamera == null || mana == null || lives == null)
            Debug.LogError("PossessionController is missing a reference.", this);

        if (puzzles == null || puzzles.Length == 0)
            Debug.LogError("PossessionController has no level puzzles.", this);

        SetWitchInControl(snapCamera: true);
        LockCursor();
    }

    void OnEnable()
    {
        if (mana != null)
            mana.Depleted += HandleManaDepleted;
        if (lives != null)
            lives.GameOver += HandleGameOver;
    }

    void OnDisable()
    {
        if (mana != null)
            mana.Depleted -= HandleManaDepleted;
        if (lives != null)
            lives.GameOver -= HandleGameOver;
    }

    void Update()
    {
        if (mana != null && mana.Current > 0.05f)
            lifeLostForThisDepletion = false;

        if (gameOver || input == null || input.Possess == null)
        {
            PromptText = "";
            return;
        }

        bool possessPressed = input.Possess.WasPressedThisFrame();

        if (controllingSkeleton)
        {
            PromptText = CarryPrompt();
            if (possessPressed)
                EndPossession();
            return;
        }

        LevelPuzzle puzzle = FindPuzzleInRange();
        bool canPossess = puzzle != null && mana != null && mana.HasMana && puzzle.SkeletonAvailable && lives != null && lives.HasLives;

        if (canPossess)
            PromptText = "Press Q to possess skeleton";
        else if (puzzle != null && mana != null && !mana.HasMana)
            PromptText = "Not enough mana";
        else
            PromptText = "";

        if (possessPressed && canPossess)
            StartPossession(puzzle);
    }

    public LevelPuzzle FindPuzzleInRange()
    {
        if (witch == null || puzzles == null)
            return null;

        LevelPuzzle nearest = null;
        float nearestDistance = float.MaxValue;
        Vector3 witchPosition = witch.transform.position;

        foreach (LevelPuzzle puzzle in puzzles)
        {
            if (puzzle == null || puzzle.Skeleton == null || !puzzle.IsWitchInRange(witchPosition))
                continue;

            float doorDistance = puzzle.DistanceToDoor(witchPosition);
            if (doorDistance < nearestDistance)
            {
                nearestDistance = doorDistance;
                nearest = puzzle;
            }
        }

        return nearest;
    }

    string CarryPrompt()
    {
        SkeletonCarrier carrier = activePuzzle != null && activePuzzle.Skeleton != null
            ? activePuzzle.Skeleton.GetComponent<SkeletonCarrier>()
            : null;
        string carry = carrier != null ? carrier.Prompt : "";
        if (string.IsNullOrEmpty(carry))
            return "Press Q to release";

        return carry + "\nPress Q to release";
    }

    void StartPossession(LevelPuzzle puzzle)
    {
        if (puzzle == null || puzzle.Skeleton == null || witch == null)
            return;

        activePuzzle = puzzle;
        controllingSkeleton = true;
        witch.SetControlEnabled(false);
        puzzle.Skeleton.SetControlEnabled(true);

        if (followCamera != null)
            followCamera.SetTarget(puzzle.Skeleton.transform, false);
        if (mana != null)
            mana.SetDraining(true);
    }

    public void EndPossession()
    {
        if (!controllingSkeleton)
            return;

        ThirdPersonMotor skeleton = activePuzzle != null ? activePuzzle.Skeleton : null;
        if (skeleton != null)
            skeleton.SetControlEnabled(false);

        controllingSkeleton = false;
        activePuzzle = null;

        if (witch != null)
            witch.SetControlEnabled(true);
        if (followCamera != null && witch != null)
            followCamera.SetTarget(witch.transform, false);
        if (mana != null)
            mana.SetDraining(false);
    }

    void HandleManaDepleted()
    {
        if (!controllingSkeleton || lifeLostForThisDepletion)
            return;

        lifeLostForThisDepletion = true;
        EndPossession();

        if (lives != null)
            lives.LoseLife();
    }

    void HandleGameOver()
    {
        gameOver = true;
        PromptText = "";

        if (controllingSkeleton)
            EndPossession();

        if (witch != null)
            witch.SetControlEnabled(false);

        if (puzzles != null)
        {
            foreach (LevelPuzzle puzzle in puzzles)
            {
                if (puzzle != null && puzzle.Skeleton != null)
                    puzzle.Skeleton.SetControlEnabled(false);
            }
        }

        if (followCamera != null)
            followCamera.LookEnabled = false;

        UnlockCursor();
    }

    public void Win()
    {
        if (gameOver || HasWon)
            return;

        HasWon = true;
        HandleGameOver();
        Won?.Invoke();
    }

    void SetWitchInControl(bool snapCamera)
    {
        controllingSkeleton = false;
        activePuzzle = null;

        if (puzzles != null)
        {
            foreach (LevelPuzzle puzzle in puzzles)
            {
                if (puzzle != null && puzzle.Skeleton != null)
                    puzzle.Skeleton.SetControlEnabled(false);
            }
        }

        if (witch != null)
            witch.SetControlEnabled(true);
        if (followCamera != null && witch != null)
            followCamera.SetTarget(witch.transform, snapCamera);
        if (mana != null)
            mana.SetDraining(false);
    }

    static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
