using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed;
    [SerializeField]
    private float hoverSpeed;

    private Vector3 startPos;
    private float timer;

    private void Start() { startPos = transform.position; UnityEngine.Cursor.lockState = CursorLockMode.Confined; }

    private void Update()
    {
        timer += Time.deltaTime * hoverSpeed;
        transform.position = startPos + Vector3.up * Mathf.Sin(timer) * 0.05f;
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
