using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private float minPanSpeed;
        [SerializeField]
        private float currPanSpeed;
        [SerializeField]
        private float maxPanSpeed;
        [SerializeField]
        private float panBoardThickness = 10f;
        [SerializeField]
        private Vector3 minClamp;
        [SerializeField]
        private Vector3 maxClamp;
        [SerializeField]
        private float scrollSpeed = 20f;

        // Update is called once per frame
        void Update()
        {
            Vector3 pos = transform.position;
            float lpos = 0;

            float t = (transform.position.y - minClamp.y) / (maxClamp.y - minClamp.y);
            currPanSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, t);

            if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - panBoardThickness)
                pos.z += currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= panBoardThickness)
                pos.z -= currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - panBoardThickness)
                pos.x += currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= panBoardThickness)
                pos.x -= currPanSpeed * Time.deltaTime;

            float scroll = Input.GetAxis("Mouse ScrollWheel");

            lpos += scrollSpeed * 100f * Time.deltaTime * scroll;

            pos += transform.forward * lpos;

            pos.x = Mathf.Clamp(pos.x, minClamp.x, maxClamp.x);
            pos.y = Mathf.Clamp(pos.y, minClamp.y, maxClamp.y);
            pos.z = Mathf.Clamp(pos.z, minClamp.z, maxClamp.z);

            transform.position = pos;
        }
    }
}
