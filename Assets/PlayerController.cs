using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    // Start is called before the first frame update
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public float radius;
    public float power;
    public GameObject hatObject;
    public Transform particleSpawner;
    public bool onCooldown = false;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if(curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }
        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();

            if (Input.GetKeyDown(KeyCode.Z))
                StartCoroutine(TryThePower());

            if (hatObject.activeInHierarchy)
                curHatTime += Time.deltaTime;
        }
        
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    IEnumerator TryThePower()
    {
        if (GameManager.instance.playerWithHat == this.id && !onCooldown)
        {
            GameManager.instance.photonView.RPC("UnleashThePower", RpcTarget.All, id);
            onCooldown = true;
            yield return new WaitForSeconds(5);
            onCooldown = false;
        }
        else
        {
            Debug.Log("you do not have the power!");
        }
    }

    public void SetHat(bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    public void SetThePower(bool hasThePower)
    {
        GameObject oldPower = GameObject.Find("Effect22(Clone)");
        Vector3 explosionPos = this.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach(Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if((rb != this.GetComponent<Rigidbody>()) && (rb != null))
            {
                rb.AddExplosionForce(power, explosionPos, radius);
            }
        }
        Destroy(oldPower);
        Instantiate(Resources.Load("Effect22"), this.transform.position, Quaternion.identity);

    }

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
            rig.isKinematic = true;

        if (id == 1)
            GameManager.instance.GiveHat(id, true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;
        if (collision.gameObject.CompareTag("Player"))
        {
            if(GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat)
            {
                if (GameManager.instance.CanGetHat())
                {
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curHatTime);
        }
        else if (stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}
