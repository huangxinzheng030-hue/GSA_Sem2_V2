using UnityEngine;

[DisallowMultipleComponent]
public class RotatePuzzleController : MonoBehaviour
{
    [Header("Rings (6)")]
    [Tooltip("Assign Ring_0..Ring_5 (Ring0 inner/smallest -> Ring5 outer/largest)")]
    public RingPiece[] rings = new RingPiece[6];

    [Header("Display Switch")]
    public GameObject ringsGroup;      // RingsGroup (broken rings container)
    public GameObject finalRingImage;  // FinalRingImage (complete image)

    [Header("Hint")]
    public HintTextController hint;

    [Header("Rotation")]
    [Tooltip("Rotate step in degrees (e.g. 15). Correct angles must be multiples of this.")]
    public float rotateStep = 15f;

    [Header("Input")]
    public KeyCode selectOuterKey = KeyCode.W; // W -> outer (index + 1)
    public KeyCode selectInnerKey = KeyCode.S; // S -> inner (index - 1)

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip tickRotate; // mouse rotate
    public AudioClip tickSelect; // W/S selection only
    public AudioClip successSfx; // optional

    [Header("Scramble")]
    public bool scrambleOnEnable = true;

    [Header("Debug")]
    public bool logSolveDebug = false;
    public bool logAudioDebug = false;

    private int selectedIndex = 0;
    private bool solved = false;
    private int maxSteps;

    void Awake()
    {
        // Robust: avoid maxSteps = 0 if rotateStep is wrong
        rotateStep = Mathf.Max(0.0001f, rotateStep);
        maxSteps = Mathf.Max(1, Mathf.RoundToInt(360f / rotateStep));
    }

    void OnEnable()
    {
        solved = false;
        selectedIndex = 0;

        // Show broken rings, hide final image
        if (ringsGroup) ringsGroup.SetActive(true);
        if (finalRingImage) finalRingImage.SetActive(false);

        // Show hidden hint text
        if (hint) hint.ShowHidden();

        // Validate references (helps catch silent failures)
        ValidateRefs();

        // Init highlight
        RefreshHighlight();

        // Random scramble
        if (scrambleOnEnable)
            Scramble();

        RefreshHighlight();

        // Optional: quick audio test log
        if (logAudioDebug)
            Debug.Log($"[Audio] sfxSource={(sfxSource ? "OK" : "NULL")}, tickSelect={(tickSelect ? "OK" : "NULL")}, tickRotate={(tickRotate ? "OK" : "NULL")}, successSfx={(successSfx ? "OK" : "NULL")}");
    }

    void Update()
    {
        if (solved) return;

        // W/S selection (ONLY these trigger tickSelect)
        if (Input.GetKeyDown(selectOuterKey))
            TrySelect(selectedIndex + 1);

        if (Input.GetKeyDown(selectInnerKey))
            TrySelect(selectedIndex - 1);

        // Mouse rotation (ONLY these trigger tickRotate)
        if (Input.GetMouseButtonDown(0))
            RotateSelected(+rotateStep);

        // Optional: right click reverse
        if (Input.GetMouseButtonDown(1))
            RotateSelected(-rotateStep);
    }

    private void ValidateRefs()
    {
        // If any ring is missing, solved check can never be reliable
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] == null)
            {
                Debug.LogWarning($"[Puzzle] rings[{i}] is NULL. Please assign Ring_{i} in the Inspector.");
            }
        }

        if (!ringsGroup) Debug.LogWarning("[Puzzle] ringsGroup is NULL. Assign RingsGroup in Inspector.");
        if (!finalRingImage) Debug.LogWarning("[Puzzle] finalRingImage is NULL. Assign FinalRingImage in Inspector.");
        if (!hint) Debug.LogWarning("[Puzzle] hint is NULL. Assign HintTextController (HintText object) in Inspector.");
        if (!sfxSource && (tickSelect || tickRotate || successSfx))
            Debug.LogWarning("[Audio] sfxSource is NULL but AudioClips are assigned. Assign SFX AudioSource.");
    }

    private void TrySelect(int newIndex)
    {
        newIndex = Mathf.Clamp(newIndex, 0, rings.Length - 1);
        if (newIndex == selectedIndex) return;

        selectedIndex = newIndex;
        RefreshHighlight();
        PlayOneShot(tickSelect); // W/S selection sound
    }

    private void RotateSelected(float step)
    {
        if (rings == null || rings.Length == 0) return;
        if (selectedIndex < 0 || selectedIndex >= rings.Length) return;
        if (rings[selectedIndex] == null) return;

        rings[selectedIndex].StepRotate(step);
        PlayOneShot(tickRotate); // rotate sound

        CheckSolved();
    }

    private void RefreshHighlight()
    {
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
                rings[i].SetHighlighted(i == selectedIndex);
        }
    }

    private void Scramble()
    {
        int safety = 0;
        do
        {
            for (int i = 0; i < rings.Length; i++)
            {
                if (rings[i] == null) continue;

                int step = Random.Range(0, maxSteps); // 0..maxSteps-1
                rings[i].SetAngle(step * rotateStep); // SetAngle will enforce 2D Z-only in RingPiece
            }

            safety++;
            if (safety > 50) break;
        }
        while (IsAlreadySolved());
    }

    private bool IsAlreadySolved()
    {
        // If any ring ref missing, treat as NOT solved (avoid false positives)
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] == null) return false;

            int cur = AngleToStep(rings[i].CurrentAngle());
            int tar = AngleToStep(rings[i].correctAngle);
            if (cur != tar) return false;
        }
        return true;
    }

    private void CheckSolved()
    {
        if (logSolveDebug) Debug.Log("[Puzzle] Checking solved...");

        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] == null)
            {
                if (logSolveDebug) Debug.LogWarning($"[Puzzle] Missing rings[{i}] reference.");
                return;
            }

            int cur = AngleToStep(rings[i].CurrentAngle());
            int tar = AngleToStep(rings[i].correctAngle);

            if (logSolveDebug) Debug.Log($"[Puzzle] Ring {i}: curStep={cur}, targetStep={tar}");

            if (cur != tar) return;
        }

        OnSolved();
    }

    // ---- Key fix: stable step conversion (prevents float errors blocking OnSolved) ----
    private int AngleToStep(float angle)
    {
        float a = Normalize(angle);

        // Snap to nearest step
        int s = Mathf.RoundToInt(a / rotateStep);

        // Wrap into [0, maxSteps)
        s = (s % maxSteps + maxSteps) % maxSteps;
        return s;
    }

    private float Normalize(float a)
    {
        a = (a % 360f + 360f) % 360f;
        return a;
    }

    private void OnSolved()
    {
        solved = true;
        if (logSolveDebug) Debug.Log("[Puzzle] SOLVED -> switching visuals + revealing hint.");

        PlayOneShot(successSfx);

        if (ringsGroup) ringsGroup.SetActive(false);
        if (finalRingImage) finalRingImage.SetActive(true);

        if (hint) hint.RevealProgressively();
    }

    // ---- Audio fix/diagnostic: clearly logs when something is missing ----
    private void PlayOneShot(AudioClip clip)
    {
        if (!clip)
        {
            if (logAudioDebug) Debug.LogWarning("[Audio] Tried to play null clip.");
            return;
        }

        if (!sfxSource)
        {
            if (logAudioDebug) Debug.LogWarning($"[Audio] No sfxSource assigned, cannot play clip: {clip.name}");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
