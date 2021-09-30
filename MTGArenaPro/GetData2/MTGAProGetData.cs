﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;

namespace GetData2
{
    public class LogElement
    {
        public string timestamp;
        public object Payload;
    }

    public class MTGAProGetData : MonoBehaviour
    {
        private static PAPA ourPapaInstance = null;
        private static bool gotInitialData = false;

        public void Start()
        {
            try
            {
                System.Random RNG = new System.Random();
                int length = 32;
                string rString = "";
                for (var i = 0; i < length; i++)
                {
                    rString += ((char)(RNG.Next(1, 26) + 64)).ToString().ToLower();
                }
                GetHoldOnPapa();
            }
            catch (Exception e)
            {
                WriteToLog("Error", e);
            }
        }

        private void GetHoldOnPapa()
        {
            try
            {
                ourPapaInstance = FindObjectOfType<PAPA>();

                if (!gotInitialData && ourPapaInstance != null && ourPapaInstance.AccountClient != null && ourPapaInstance.AccountClient.AccountInformation != null && ourPapaInstance.InventoryManager != null && ourPapaInstance.InventoryManager.Cards != null && ourPapaInstance.InventoryManager.Cards.Count > 0)
                {
                    GetInitialData();
                    return;
                }
                else if (!gotInitialData)
                {
                    ourPapaInstance = null;
                    GetHoldOnPapa();
                }
            }
            catch (Exception e)
            {
                WriteToLog("ErrorGetHoldOnPapa", e);
            }
        }

        private void WriteToLog(string indicator, object report)
        {
            try
            {
                LogElement logElem = new LogElement
                {
                    Payload = report,
                    timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()
                };
                Debug.Log($"[MTGA.Pro Logger] **{indicator}** {JsonConvert.SerializeObject(logElem)}");
            }
            catch (Exception e)
            {
                Debug.Log($"[MTGA.Pro Logger] **WriteToLogError** {e}");
            }
        }

        private void GetInitialData()
        {
            try
            {
                gotInitialData = true;
                ourPapaInstance.InventoryManager.UnsubscribeFromAll(InventoryChangeHandler);
                ourPapaInstance.InventoryManager.SubscribeToAll(InventoryChangeHandler);
                ourPapaInstance.AccountClient.LoginStateChanged += AccountClient_LoginStateChanged;
                InventoryManager inventory = ourPapaInstance.InventoryManager;
                WriteToLog("Userdata", new { userId = ourPapaInstance.AccountClient.AccountInformation.AccountID, screenName = ourPapaInstance.AccountClient.AccountInformation.DisplayName });
                WriteToLog("Collection", inventory.Cards);
                WriteToLog("InventoryContent", inventory.Inventory);
                Task task = new Task(() => PeriodicCollectionPrinter());
                task.Start();
            }
            catch (Exception e)
            {
                WriteToLog("ErrorGetInitialData", e);
            }
        }

        private void PeriodicCollectionPrinter()
        {
            if (ourPapaInstance != null)
            {
                Thread.Sleep(600000);
                InventoryManager inventory = ourPapaInstance.InventoryManager;
                WriteToLog("Collection", inventory.Cards);
                WriteToLog("InventoryContent", inventory.Inventory);
            }
        }

        private void AccountClient_LoginStateChanged(LoginState obj)
        {
            try
            {
                WriteToLog("LoginStateChanged", obj);
                gotInitialData = false;
                ourPapaInstance = null;
                Task task = new Task(() => GetHoldOnPapa());
                task.Start();
            }
            catch (Exception e)
            {
                WriteToLog("ErrorLoginStateChanged", e);
            }
        }

        private void InventoryChangeHandler(ClientInventoryUpdateReportItem obj)
        {
            try
            {
                WriteToLog("InventoryUpdate", obj);

                InventoryManager inventory = ourPapaInstance.InventoryManager;
                WriteToLog("Collection", inventory.Cards);
                WriteToLog("InventoryContent", inventory.Inventory);
            }
            catch (Exception e)
            {
                WriteToLog("ErrorInventoryChangeHandler", e);
            }
        }
    }
}