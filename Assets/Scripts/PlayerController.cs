using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    float tubeRadius;
    [SerializeField]
    float controlSensetivity;
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom != null)
            gameObject.GetComponent<Camera>().rect = new Rect(0, photonView.IsMine ? 0 : 0.5f, 1, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!ObstacleManager.Instance.IsGameActive || PhotonNetwork.CurrentRoom != null && !photonView.IsMine)
            return;

        Vector3 delta = Vector3.zero;
        if (Input.touches.Length > 0)
            delta = new Vector3(Input.touches[0].deltaPosition.x, 0.0f, Input.touches[0].deltaPosition.y) * controlSensetivity;
        else
            delta = new Vector3(Input.GetAxis("Mouse X"), 0.0f, Input.GetAxis("Mouse Y")) * controlSensetivity * 250 * (Input.GetMouseButton(0) ? 1 : 0);

        if (delta.sqrMagnitude != 0.0f)
        {
            Vector3 newPosition = gameObject.transform.position + delta;
            if (newPosition.sqrMagnitude > tubeRadius * tubeRadius)
                newPosition = newPosition.normalized * tubeRadius;

            gameObject.transform.position = newPosition;
        }
    }
}
