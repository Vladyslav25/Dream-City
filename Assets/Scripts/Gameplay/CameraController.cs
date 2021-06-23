using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

        private int pointerID = -1;

        private void Awake()
        {
#if !UNITY_EDITOR
            pointerID = 0;  // needs to be in a standalone version 0 to check if the mouse is hovering over UI
#endif
        }

        // Update is called once per frame
        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject(pointerID)) return;

            Vector3 pos = transform.position;
            float lpos = 0;

            float t = (transform.position.y - minClamp.y) / (maxClamp.y - minClamp.y);
            currPanSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, t);

            if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - panBoardThickness)
                pos.z += currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= panBoardThickness)
                pos.z -= currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - panBoardThickness)
                pos.x += currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= panBoardThickness)
                pos.x -= currPanSpeed * Time.deltaTime;

            float scroll = Input.GetAxis("Mouse ScrollWheel");

            lpos += scrollSpeed * 100f * Time.deltaTime * scroll;

            pos += transform.forward * lpos;

            //Prevent the camera move forward or backwards if its reached the max and min hight
            if (pos.y < minClamp.y || pos.y > maxClamp.y)
            {
                pos.y = Mathf.Clamp(pos.y, minClamp.y, maxClamp.y);
                pos.x = transform.position.x;
                pos.z = transform.position.z;
                transform.position = pos;
                return;
            }

            pos.x = Mathf.Clamp(pos.x, minClamp.x, maxClamp.x);
            pos.y = Mathf.Clamp(pos.y, minClamp.y, maxClamp.y);
            pos.z = Mathf.Clamp(pos.z, minClamp.z, maxClamp.z);


            transform.position = pos;
        }
    }
}
