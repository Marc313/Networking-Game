using System.Threading.Tasks;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public AnimationCurve rotationCurve;

    /// <summary>
    /// Moves the current object from oldPos to a new position within a given period of time.
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="targetPos"></param>
    /// <param name="timeInSeconds"></param>
    public async Task MoveToInSeconds(Vector3 oldPos, Vector3 targetPos, float timeInSeconds)
    {
        transform.position = oldPos;
        float distance = Vector3.Distance(oldPos, targetPos);
        float moveSpeed = distance / timeInSeconds;
        float t = 0;

        while (t < timeInSeconds)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            t += Time.deltaTime;
            await Task.Yield();
        }
        transform.position = targetPos;

        //onDone?.Invoke();
    }

    /**
     * Rotates the object from one orientation to another with a certain rotation speed.
     */
    public async Task RotateTowardsInSeconds(Quaternion oldRotation, Quaternion targetRotation, float timeInSeconds)
    {
        float angle = Quaternion.Angle(oldRotation, targetRotation);
        float rotationSpeed = angle / timeInSeconds;
        float t = 0;

        transform.rotation = oldRotation;

        while (t < timeInSeconds)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            t += Time.deltaTime;
            await Task.Yield();
        }
        transform.rotation = targetRotation;

        //onDone?.Invoke();
    }

    /**
     * Rotates the object from one orientation to another with a certain rotation speed.
     */
    public async Task RotateWithSlerpInSeconds(Quaternion oldRotation, Quaternion targetRotation, float timeInSeconds)
    {
        //float angle = Quaternion.Angle(closedRotation, targetRotation);
        //float rotationSpeed = angle / timeInSeconds;
        float t = 0;

        transform.rotation = oldRotation;

        while (t < timeInSeconds)
        {
            float timeRatio = timeInSeconds / t;
            transform.rotation = Quaternion.Slerp(oldRotation, targetRotation, rotationCurve.Evaluate(t));
            t += Time.deltaTime;
            await Task.Yield();
        }
        transform.rotation = targetRotation;

        //onDone?.Invoke();
    }
}


