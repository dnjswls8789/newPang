using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class PhotonManager : SingletonClass<PhotonManager>
{
    PhotonView pv;
    PhotonTransformView ptv;

    protected override void Awake()
    {
        pv = gameObject.GetComponent<PhotonView>();

        if (pv == null)
        {
            pv = gameObject.AddComponent<PhotonView>();
            PhotonNetwork.AllocateViewID(pv);
            
        }

        ptv = gameObject.GetComponent<PhotonTransformView>();

        if (ptv == null)
        {
            ptv = gameObject.AddComponent<PhotonTransformView>();
        }
    }

    public GameObject InstantiateWithPhoton(GameObject parentObject, string prefabName, Vector3 position, float invokeTime = -1.0f)
    {
        GameObject instantiatedObject = parentObject.AddChildFromObjPool(prefabName, invokeTime);
        //instantiatedObject.transform.SetPositionAndRotation(position, rotation);
        instantiatedObject.transform.position = position;
        PhotonView _pv = instantiatedObject.GetComponent<PhotonView>();
        if (_pv == null)
        {
            _pv = AddPhotonViewComponent(instantiatedObject);
        }

        _pv.TransferOwnership(PhotonNetwork.LocalPlayer);

        if (PhotonNetwork.AllocateViewID(_pv))
        {
            pv.RPC("InstantiatePrefabWithPhoton", RpcTarget.Others, prefabName, _pv.ViewID, 
                instantiatedObject.transform.position, PhotonNetwork.LocalPlayer, invokeTime);
#if UNITY_EDITOR
            Debug.Log(instantiatedObject.name);
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to allocate a ViewId.");
#endif

            Destroy(instantiatedObject);
        }

        return instantiatedObject;
    }

    public void InitializeObjectPoolWithPhoton(string _prefabName, int addPool, int minPool, int maxPoolSize)
    {
        ObjectPoolManager.GetInstance.InitializeObjPool(ObjectPoolManager.GetInstance.GetPrefab(_prefabName), addPool, minPool, maxPoolSize);
      
        pv.RPC("InitializeObjPoolWithPhoton", RpcTarget.Others, _prefabName, addPool, minPool, maxPoolSize);
    }

    [PunRPC]
    private void InitializeObjPoolWithPhoton(string _prefabName, int addPool, int minPool, int maxPoolSize)
    {
        ObjectPoolManager.GetInstance.InitializeObjPool(ObjectPoolManager.GetInstance.GetPrefab(_prefabName), addPool, minPool, maxPoolSize);
        ptv.m_SynchronizePosition = false;
        ptv.m_SynchronizeRotation = false;
    }

    [PunRPC]
    private void InstantiatePrefabWithPhoton(string _prefabName, int viewId, Vector3 _position, Player sendPlayer, float invokeTime = -1.0f)
    {
        GameObject instantiatedObject = gameObject.AddChildFromObjPool(_prefabName, invokeTime);
        //instantiatedObject.transform.SetPositionAndRotation(_position, _rotation);
        instantiatedObject.transform.position = _position;

        PhotonView photonView = instantiatedObject.GetComponent<PhotonView>();

        if (photonView == null)
        {
            photonView = instantiatedObject.AddComponent<PhotonView>();
        }

        PhotonTransformView photonTransformView = instantiatedObject.GetComponent<PhotonTransformView>();

        if (photonTransformView == null)
        {
            photonTransformView = instantiatedObject.AddComponent<PhotonTransformView>();
        }
      
        photonView.ViewID = viewId;
        photonView.TransferOwnership(sendPlayer);

        photonTransformView.m_SynchronizePosition = true;
        photonTransformView.m_SynchronizeRotation = true;
    }

    public PhotonView AddPhotonViewComponent(GameObject targetObject)
    {
        PhotonView pv = targetObject.AddComponent<PhotonView>();

        return pv;
    }

    public GameObject CharacterInstantiateWithPhoton(GameObject parentObject, string characterName)
    {
        GameObject instantiatedObject = parentObject.AddChildFromObjPool(characterName, -1.0f);
        //instantiatedObject.transform.SetPositionAndRotation(position, rotation);
        instantiatedObject.transform.position = Vector3.zero;

        PhotonView _pv = instantiatedObject.GetComponent<PhotonView>();
        if (_pv == null)
        {
            _pv = AddPhotonViewComponent(instantiatedObject);
        }

        if (PhotonNetwork.AllocateViewID(_pv))
        {
            pv.RPC("CharacterInstantiate", RpcTarget.Others, characterName.ConvertStringToByteArray(), _pv.ViewID,
                instantiatedObject.transform.position, PhotonNetwork.LocalPlayer, -1.0f);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to allocate a ViewId.");
#endif
            Destroy(instantiatedObject);
        }

        return instantiatedObject;
    }

    [PunRPC]
    private void CharacterInstantiate(byte[] characterName, int viewId, Vector3 _position, Player sendPlayer, float invokeTime)
    {
        GameObject instantiatedObject = gameObject.AddChildFromObjPool(characterName.ConvertByteArrayToString(), invokeTime);
        //instantiatedObject.transform.SetPositionAndRotation(_position, _rotation);
        instantiatedObject.transform.position = _position;

        PhotonView photonView = instantiatedObject.GetComponent<PhotonView>();

        if (photonView == null)
        {
            photonView = instantiatedObject.AddComponent<PhotonView>();
        }

        PhotonTransformView photonTransformView = instantiatedObject.GetComponent<PhotonTransformView>();

        if (photonTransformView == null)
        {
            photonTransformView = instantiatedObject.AddComponent<PhotonTransformView>();
        }

        photonView.ViewID = viewId;
        photonView.TransferOwnership(sendPlayer);

        photonTransformView.m_SynchronizePosition = true;
        photonTransformView.m_SynchronizeRotation = true;


        Renderer[] m;
        m = instantiatedObject.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < m.Length; i++)
        {
            if (m[i].material.HasProperty("_OutlineColor"))
            {
                Material mat = Instantiate(m[i].material);
                mat.SetColor("_OutlineColor", Color.red);

                m[i].material = mat;
            }
        }
    }
}
