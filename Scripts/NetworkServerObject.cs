using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Version_Control.Scripts.Shared
{
    public class NetworkServerObject : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Id of the network object
        /// </summary>
        [HideInInspector]
        public int id;
        #endregion
        #region Unity Callbacks
        private void Start()
        {
            if (!Equals(GameServerManager.Instance, null))
            {
                // Get the instance id of the gameobject on the server scene
                id = GetInstanceID();
                //Register with the server
                //GameServerManager.instance.RegisterNetworkObject(this);
            }
           

        }
        #endregion
    }
}
