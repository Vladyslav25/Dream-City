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
        [SerializeField]
        private float rotationSpeed = 10f;

        private int pointerID = -1;

        // Update is called once per frame
        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject(pointerID)) return;

            Vector3 pos = transform.position;
            float lpos = 0;

            //Lerp PanSpeed -> Faster if up in the air and slower on the ground
            float t = (transform.position.y - minClamp.y) / (maxClamp.y - minClamp.y);
            currPanSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, t);

            if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - panBoardThickness)
                pos += Vector3.Scale(new Vector3(1, 0, 1), transform.forward).normalized * currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= panBoardThickness)
                pos -= Vector3.Scale(new Vector3(1, 0, 1), transform.forward).normalized * currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - panBoardThickness)
                pos += Vector3.Scale(new Vector3(1, 0, 1), transform.right).normalized * currPanSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= panBoardThickness)
                pos -= Vector3.Scale(new Vector3(1, 0, 1), transform.right).normalized * currPanSpeed * Time.deltaTime;

            //Zoom In_Out
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            lpos += scrollSpeed * 100f * Time.deltaTime * scroll;
            pos += (Tools.ToolManager.GetHitPos() - transform.position).normalized * lpos;

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

            //Rotation
            float RotationInput = 0f;
            bool RotateInput = false;
            if (Input.GetKey(KeyCode.X))
            {
                RotationInput += rotationSpeed * Time.deltaTime;
                RotateInput = true;
            }
            if (Input.GetKey(KeyCode.Z))
            {
                RotationInput -= rotationSpeed * Time.deltaTime;
                RotateInput = true;
            }

            if (RotateInput && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
            {
                Vector3 raycastHitPos = hit.point;
                transform.RotateAround(raycastHitPos, Vector3.up, RotationInput);
            }
        }
    }
}
