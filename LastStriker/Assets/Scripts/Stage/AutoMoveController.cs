
using UnityEngine;
using System.Collections;
using System;

// Moves the rail camera through a list of waypoints (position + rotation)
// at a constant travel speed, turning smoothly between waypoint orientations.
public class AutoMoveController : MonoBehaviour
{
    public float moveSpeed = 9f;
    public float turnSpeed = 90f;
    public float minSegmentDuration = 0.05f;

    Coroutine moveRoutine;
    public bool IsMoving { get; private set; }

    public void MoveAlongPath(Transform[] path, Action onComplete)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(PathRoutine(path, onComplete));
    }

    IEnumerator PathRoutine(Transform[] path, Action onComplete)
    {
        IsMoving = true;
        foreach (Transform wp in path)
        {
            if (wp == null) continue;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 endPos = wp.position;
            Quaternion endRot = wp.rotation;

            float dist = Vector3.Distance(startPos, endPos);
            float angle = Quaternion.Angle(startRot, endRot);
            float duration = Mathf.Max(dist / moveSpeed, angle / turnSpeed, minSegmentDuration);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                transform.position = Vector3.Lerp(startPos, endPos, p);
                transform.rotation = Quaternion.Slerp(startRot, endRot, Mathf.SmoothStep(0f, 1f, p));
                yield return null;
            }
            transform.position = endPos;
            transform.rotation = endRot;
        }
        IsMoving = false;
        onComplete?.Invoke();
    }
}
