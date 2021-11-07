using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TutorialInventoryManager : SingletonGeneric<TutorialInventoryManager>
{
    public List<InventoryItem> itemListReference;
    [SerializeField] private GameObject inventoryPanel;
    // All money texts go here
    [SerializeField] private List<TextMeshProUGUI> moneyTexts;
    private InventorySlot[] slots;
    private int inventoryLimit = 20;
    // Cache the inventory dictionary generated by InventoryToDictionary for performance reasons
    private Dictionary<int, Tuple<int, List<InventoryItem>>> inventoryDictionaryCache;
    private bool isCacheValid = false;

    // For event trigger
    public InventoryItem requiredPotato;
    public bool hasCollected3Potatoes = false;
    public InventoryItem requiredCookedDish;
    public bool hasCookedRequiredDish = false;

    private void Start()
    {
        slots = inventoryPanel.GetComponentsInChildren<InventorySlot>();
        PopulateUI(PlayerManager.instance.itemList);

        TutorialManager.OnRestartTutorial += () =>
        {
            hasCollected3Potatoes = false;
            hasCookedRequiredDish = false;
        };
    }

    public void PopulateUI(List<InventoryItem> items)
    {
        itemListReference = items;
        //UIController.UpdateAllUIs();
    }

    public void ForceUIUpdate()
    {
        StopAllCoroutines();
        StartCoroutine(UpdateUI());
    }
    private IEnumerator UpdateUI()
    {
        // UpdateWeaponSkillStats();
        // healthPill.text = "x " + PlayerManager.instance.healthPill;
        // staminaPill.text = "x " + PlayerManager.instance.staminaPill;
        // potion.text = "x " + PlayerManager.instance.potion;
        // pfizerShot.text = "x " + PlayerManager.instance.pfizerShot;
        // modernaShot.text = "x " + PlayerManager.instance.modernaShot;
        string currentMoney = PlayerManager.instance.currentMoney.ToString();
        foreach (TextMeshProUGUI text in moneyTexts)
        {
            text.text = currentMoney;
        }
        // healthAmount.text = Mathf.CeilToInt(PlayerManager.instance.currentHealth) + "/" + Mathf.CeilToInt(PlayerManager.instance.maxHealth);

        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                yield return null;
                if (i < itemListReference.Count)
                {
                    slots[i].AddItem(itemListReference[i]);
                }
                else
                {
                    slots[i].ClearSlot();
                }
            }
        }
    }

    public bool CheckIfItemsExist(List<(int, int)> itemsList, out List<(int, int, int)> outInvReqCounter)
    {
        Dictionary<int, Tuple<int, List<InventoryItem>>> itemsInInventory = InventoryToDictionary();
        List<(int, int, int)> inventoryCountToRequiredCount = new List<(int, int, int)>();
        bool doAllItemsExist = true;
        foreach ((int itemId, int reqCount) in itemsList)
        {
            int inventoryCount = 0;
            // Check the inventory for the items
            if (itemsInInventory.ContainsKey(itemId))
            {
                inventoryCount = itemsInInventory[itemId].Item1;
            }
            if (inventoryCount < reqCount)
            {
                doAllItemsExist = false;
            }
            inventoryCountToRequiredCount.Add((itemId, inventoryCount, reqCount));
        }
        outInvReqCounter = inventoryCountToRequiredCount;
        return doAllItemsExist;
    }

    // Checks if the inventory has this item
    public bool CheckIfExists(int itemId)
    {
        foreach (InventoryItem i in itemListReference)
        {
            if (i.inventoryItemId == itemId)
            {
                return true;
            }
        }
        return false;
    }

    // Tries to remove an item, given the ID.
    // If the item was not found, return false.
    // Otherwise, remove the item and return true.
    //
    // Does not call UpdateAllUis, as completing the order will call it
    public bool RemoveIdIfPossible(int idToRemove)
    {
        for (int i = 0; i < itemListReference.Count; i++)
        {
            InventoryItem currentItem = itemListReference[i];
            if (currentItem.inventoryItemId == idToRemove)
            {
                itemListReference.RemoveAt(i);
                // isCacheValid = false;
                return true;
            }
        }
        return false;
    }

    private Dictionary<int, Tuple<int, List<InventoryItem>>> InventoryToDictionary()
    {
        if (!isCacheValid)
        {
            Dictionary<int, Tuple<int, List<InventoryItem>>> itemsInInventory =
                new Dictionary<int, Tuple<int, List<InventoryItem>>>();

            foreach (InventoryItem i in itemListReference)
            {
                if (itemsInInventory.ContainsKey(i.inventoryItemId))
                {
                    // need to do this because tuples are read-only
                    Tuple<int, List<InventoryItem>> originalPair = itemsInInventory[i.inventoryItemId];
                    originalPair.Item2.Add(i);
                    itemsInInventory[i.inventoryItemId] = new Tuple<int, List<InventoryItem>>(
                        originalPair.Item1 + 1,
                        originalPair.Item2
                    );
                }
                else
                {
                    List<InventoryItem> itemsForThatId = new List<InventoryItem>();
                    itemsForThatId.Add(i);
                    itemsInInventory.Add(i.inventoryItemId, new Tuple<int, List<InventoryItem>>(1, itemsForThatId));
                }
            }
            inventoryDictionaryCache = itemsInInventory;
            isCacheValid = true;
        }
        return inventoryDictionaryCache;
    }

    // Counts how many items of a certain type there are in the inventory
    public int GetAmountOfItem(int itemId)
    {
        int amount = 0;
        foreach (InventoryItem i in itemListReference)
        {
            if (i.inventoryItemId == itemId)
            {
                amount++;
            }
        }
        return amount;
    }

    // Checks if the item IDs specified exist in the inventory.
    // If they do, remove them and return true.
    // Otherwise, returns false.
    // 
    // NOTE: This does not call UpdateUI(), because adding the final dish to the inventory would also do so.
    public bool RemoveIdsFromInventory(List<(int, int)> itemsToRemove)
    {
        if (!CheckIfItemsExist(itemsToRemove, out _))
        {
            return false;
        }

        // Remove the items, here we are guaranteed to have enough
        Dictionary<int, Tuple<int, List<InventoryItem>>> itemsInInventory = InventoryToDictionary();
        foreach ((int itemId, int count) in itemsToRemove)
        {
            List<InventoryItem> inventoryItems = itemsInInventory[itemId].Item2;
            for (int i = 0; i < count; i++)
            {
                // just remove first one every time
                itemListReference.Remove(inventoryItems[0]);
                inventoryItems.Remove(inventoryItems[0]);
            }
        }
        return true;
    }

    // Adds an item and updates inventory, recipe and order UIs
    public bool AddItem(InventoryItem item)
    {
        if (itemListReference.Count < inventoryLimit)
        {
            itemListReference.Add(item);
            isCacheValid = false;
            // May affect Recipe, Orders UI as well
            TutorialUIController.UpdateAllUIs();

            Debug.Log("inv manager: num of potatoes: " + itemListReference.FindAll((item) => item.itemName.Equals(requiredPotato.itemName)).Count);
            if (itemListReference.FindAll((item) => item.itemName.Equals(requiredPotato.itemName)).Count == 3)
            {
                hasCollected3Potatoes = true;
            }
            if (itemListReference.Contains(requiredCookedDish))
            {
                hasCookedRequiredDish = true;
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}