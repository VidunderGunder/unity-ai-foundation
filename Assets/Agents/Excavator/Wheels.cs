using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheels : MonoBehaviour
{
    public GameObject wheelVisualPrefab;
    public GameObject wheelColliderPrefab;

    public Vector3 positionOfFrontWheels;
    public Vector3 scaleOfBackWheels;
    public Vector3 rotationOfFrontWheels;

    public Vector3 positionOfBackWheels;
    public Vector3 scaleOfFrontWheels;
    public Vector3 rotationOfBackWheels;

    List<GameObject> wheelVisuals;
    List<GameObject> wheelColliders;

    private void SyncWheels()
    {
        // if (wheelVisuals.Count > 0)
        // {
        //     foreach (var wheelVisual in wheelVisuals)
        //     {
        //         if (Application.isPlaying)
        //         {
        //             Destroy(wheelVisual);
        //         }
        //         else
        //         {
        //             DestroyImmediate(wheelVisual);
        //         }
        //     }
        // }
        // if (wheelColliders.Count > 0)
        // {
        //     foreach (var wheelCollider in wheelColliders)
        //     {
        //         if (Application.isPlaying)
        //         {
        //             Destroy(wheelCollider);
        //         }
        //         else
        //         {
        //             DestroyImmediate(wheelCollider);
        //         }
        //     }
        // } 

        for (int i = 0; i < 4; i++)
        {
            GameObject wheelVisual;
            GameObject wheelCollider;

            if (wheelVisuals.Count == 4)
            {
                wheelVisual = wheelVisuals[i];
            }
            else
            {
                wheelVisual = Instantiate(wheelVisualPrefab);

            }
            if (wheelColliders.Count == 4)
            {
                wheelCollider = wheelColliders[i];
            }
            else
            {
                wheelCollider = Instantiate(wheelColliderPrefab);
            }

            wheelVisuals.Add(wheelVisual);
            wheelColliders.Add(wheelCollider);

            switch (i)
            {
                case 0:
                    wheelCollider.transform.position = wheelVisual.transform.position = positionOfFrontWheels;
                    break;
                case 1:
                    wheelCollider.transform.position
                        = wheelVisual.transform.position
                        = new Vector3(
                            -positionOfFrontWheels.x,
                            positionOfFrontWheels.y,
                            positionOfFrontWheels.z
                        );
                    break;
                case 2:
                    wheelCollider.transform.position = wheelVisual.transform.position = positionOfBackWheels;
                    break;
                case 3:
                    wheelCollider.transform.position
                        = wheelVisual.transform.position
                        = new Vector3(
                            -positionOfBackWheels.x,
                            positionOfBackWheels.y,
                            positionOfBackWheels.z
                        );
                    break;
                default:
                    break;
            }
        }
    }

    private void OnValidate()
    {
        SyncWheels();
    }
}
