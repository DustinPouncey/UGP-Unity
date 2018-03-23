﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
namespace UGP
{

    public class ItemBehaviour : NetworkBehaviour
    {
        public Text ItemName;
        public GameObject ItemCanvas;
        public Item ItemConfig;
        [HideInInspector]
        public Item _I;

        private Rigidbody rb;
        public Transform _parent;
        private PlayerInteractionBehaviour player;
        private NetworkIdentity item_network_identity;
        [SyncVar] public bool isBeingHeld = false;
        
        public void PickUp(Transform parent, PlayerInteractionBehaviour interaction)
        {
            player = interaction;
            player.CmdSetHolding(true);

            player.CmdAssignItemAuthority(item_network_identity);
            Debug.Log("ATTEMPT TO ASSIGN AUTHORITY: EXPECTED = True, RESULT = " + item_network_identity.hasAuthority.ToString());

            //PARENT THE 'ITEM' TO THE PLAYER
            //TURN GRAVITY OFF FOR THE ITEM
            _parent = parent;
            transform.SetParent(_parent);
            interaction.Child.target = transform;
            var item_pos = Vector3.zero;
            //var item_pos = _parent.TransformPoint(Vector3.zero);
            //rb.MovePosition(item_pos);
            rb.MovePosition(_parent.position);
            //rb.position = _parent.position;

            rb.constraints = RigidbodyConstraints.FreezeAll;
            //GetComponent<BoxCollider>().enabled = false;
            rb.useGravity = false;
            isBeingHeld = true;

            
        }

        public void Drop(Transform parent)
        {
            player.CmdRemoveItemAuthority(item_network_identity);
            Debug.Log("ATTEMPT TO REMOVE AUTHORITY: EXPECTED = False, RESULT = " + item_network_identity.hasAuthority.ToString());

            player.Child.target = player.originalChild;
            player.CmdSetHolding(false);
            player = null;

            isBeingHeld = false;
            transform.parent = null;
            _parent = null;

            //transform.parent = null;
            //var item_pos = _parent.position;
            //var item_pos = _parent.position;
            //rb.MovePosition(item_pos);

            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            rb.MovePosition(parent.position);
        }

        //public void UseItem()
        //{

        //}

        private void OnTriggerStay(Collider other)
        {
            if (other.tag == "Player")
            {
                var player_identity = other.GetComponent<NetworkIdentity>();

                if(player_identity.isLocalPlayer && !isBeingHeld)
                {
                    Debug.Log("Press F To Pick Up " + _I.name);
                    ItemName.text = ItemConfig.name;
                    ItemCanvas.SetActive(true);
                }

                var player_holding_transform = other.transform.Find("ItemHoldPosition");

                var player_interaction = other.GetComponent<PlayerInteractionBehaviour>();

                if (player_interaction != null)
                {
                    player = player_interaction;
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (!player_interaction.isHolding && !isBeingHeld && !isServer)
                    {
                        PickUp(player_holding_transform, player);
                    }
                }
            }

            if (other.tag == "Vehicle")
            {
                if (isBeingHeld)
                {
                    //TYPE CAST THE ITEM CONFIG AS A REPAIR KIT
                    //INVOKE THE FUNCTION 'TakeHealth' ON THE VEHICLE
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            ItemCanvas.SetActive(false);
        }

        void Start()
        {
            if (_I == null)
            {
                if (ItemConfig == null)
                {
                    ItemConfig = Resources.Load("Assets\\Resources\\ScriptableObjects\\Items\\DefaultItem") as Item;
                }
                else
                {
                    _I = Instantiate(ItemConfig);
                }
            }

            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            item_network_identity = GetComponent<NetworkIdentity>();
            if(item_network_identity == null)
            {
                item_network_identity = gameObject.AddComponent<NetworkIdentity>();
            }
        }

        void FixedUpdate()
        {
            if (isBeingHeld && !isServer && hasAuthority)
            {
                ItemCanvas.SetActive(false);
                //var item_pos = _parent.position;
                //rb.MovePosition(item_pos);
                rb.MovePosition(_parent.position);
                //rb.position = _parent.position;
                rb.rotation = _parent.rotation;
            }

            if (Input.GetKeyDown(KeyCode.RightAlt))
            {
                if (isBeingHeld && !isServer && hasAuthority)
                {
                    //var player_interaction = other.GetComponent<PlayerInteractionBehaviour>();
                    Drop(_parent);
                }
            }
        }
    }
}
