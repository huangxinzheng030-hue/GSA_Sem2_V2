using UnityEngine;
using System.Collections;

public enum PieceColor
{
    White,
    Black
}

public enum PieceType
{
    King,
    Queen,
    Rook,
    Bishop,
    Knight,
    Pawn
}

public class ChessPiece : MonoBehaviour
{
    public PieceColor color;
    public PieceType type;
    public BoardSquare currentSquare;

    [Header("Position")]
    public Vector3 positionOffset;
    public float selectedLiftHeight = 0.25f;
    public bool autoCapturePositionOffset = true;

    [Header("Move Animation")]
    public float moveDuration = 0.25f;

    [Header("Hover Animation")]
    public float hoverAmplitude = 0.03f;
    public float hoverFrequency = 2f;

    [Header("Selection Visuals")]
    public GameObject[] selectionVisuals;

    private Coroutine moveRoutine;
    private bool isHovering = false;

    private void Start()
    {
        if (autoCapturePositionOffset && currentSquare != null)
        {
            positionOffset = transform.position - currentSquare.transform.position;
        }
    }

    private Vector3 GetSnappedPosition(BoardSquare square)
    {
        return square.transform.position + positionOffset;
    }

    public void SnapToSquare(BoardSquare square)
    {
        currentSquare = square;
        isHovering = false;
        StopMoveRoutine();
        transform.position = GetSnappedPosition(square);
    }

    public IEnumerator SmoothSnapToSquareRoutine(BoardSquare square)
    {
        currentSquare = square;
        isHovering = false;
        StopMoveRoutine();

        Vector3 startPos = transform.position;
        Vector3 targetPos = GetSnappedPosition(square);
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            yield return null;
        }

        transform.position = targetPos;
    }

    public void LiftUp()
    {
        if (currentSquare == null) return;

        isHovering = true;
        Vector3 targetPos = GetSnappedPosition(currentSquare) + Vector3.up * selectedLiftHeight;

        StopMoveRoutine();
        moveRoutine = StartCoroutine(LiftAndHover(targetPos));
    }

    public void PutDown()
    {
        if (currentSquare == null) return;

        isHovering = false;
        Vector3 targetPos = GetSnappedPosition(currentSquare);

        StopMoveRoutine();
        moveRoutine = StartCoroutine(SmoothMoveTo(targetPos));
    }

    public void SetSelectionEffects(bool isOn)
    {
        if (selectionVisuals == null) return;

        foreach (GameObject visual in selectionVisuals)
        {
            if (visual != null)
                visual.SetActive(isOn);
        }
    }

    public void RestoreState(BoardSquare square, Vector3 worldPosition, Quaternion worldRotation)
    {
        currentSquare = square;
        isHovering = false;
        StopMoveRoutine();
        transform.position = worldPosition;
        transform.rotation = worldRotation;
    }

    public void SmoothRestoreState(BoardSquare square, Vector3 worldPosition, Quaternion worldRotation)
    {
        currentSquare = square;
        isHovering = false;

        StopMoveRoutine();
        moveRoutine = StartCoroutine(SmoothRestoreRoutine(worldPosition, worldRotation));
    }

    public IEnumerator SmoothRestoreStateRoutine(BoardSquare square, Vector3 worldPosition, Quaternion worldRotation)
    {
        currentSquare = square;
        isHovering = false;
        StopMoveRoutine();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, worldPosition, easedT);
            transform.rotation = Quaternion.Slerp(startRot, worldRotation, easedT);

            yield return null;
        }

        transform.position = worldPosition;
        transform.rotation = worldRotation;
    }

    private void StopMoveRoutine()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    private IEnumerator SmoothMoveTo(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            yield return null;
        }

        transform.position = targetPos;
        moveRoutine = null;
    }

    private IEnumerator SmoothRestoreRoutine(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, easedT);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        moveRoutine = null;
    }

    private IEnumerator LiftAndHover(Vector3 topPos)
    {
        Vector3 startPos = transform.position;
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, topPos, easedT);
            yield return null;
        }

        transform.position = topPos;

        float hoverTime = 0f;
        while (isHovering)
        {
            hoverTime += Time.deltaTime;
            float offsetY = Mathf.Sin(hoverTime * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;

            transform.position = topPos + Vector3.up * offsetY;
            yield return null;
        }

        moveRoutine = null;
    }
}