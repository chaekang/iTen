using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class InventoryManager : MonoBehaviourPun
{
    public GameObject inventoryUI;
    public Animator inventoryAnimator;
    public Slot[] slots;
    public Transform player;
    public float pickupRange = 2f;
    private bool isInventoryOpen = false;
    private int selectedSlotIndex = -1;
    public LayerMask itemLayer = 8;

    private void Start()
    {
        inventoryUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }

        if (isInventoryOpen)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectSlot(i);
                    break;
                }
            }

            if (Input.GetMouseButtonDown(1) && selectedSlotIndex >= 0)
            {
                DropItem(selectedSlotIndex);
                //UseItem(selectedSlotIndex);
            }
        }

        // .������ �ݱ�
        if (Input.GetMouseButtonDown(0))
        {
            TryPickupItem();
        }
        if (Input.GetMouseButtonDown(0))
        {
            ItemObject.CheckAndInteract();
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (isInventoryOpen)
        {
            inventoryUI.SetActive(true);
            inventoryAnimator.SetBool("IsOpen", true);
        }
        else
        {
            inventoryAnimator.SetBool("IsOpen", false);
            StartCoroutine(DisableInventoryAfterDelay(1f));
        }
    }

    private IEnumerator DisableInventoryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        inventoryUI.SetActive(false);
    }

    private void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Highlight(i == index);
        }
    }

    private void UseItem(int index)
    {
        Slot slot = slots[index];
        if (slot.HasItem)
        {
            slot.UseItem();
            if (slot.ItemCount <= 0)
            {
                slot.ClearSlot();
            }
        }
    }

    private void TryPickupItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red, 0.5f);
        
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
        {
            ItemObject item = hit.collider.GetComponent<ItemObject>();
            if (item != null)
            {
                if (item is SoundBox soundBox && soundBox.IsDropped)
                {
                    Debug.Log("Cannot pick up a dropped SoundBox.");
                    return;
                }

                Debug.Log($"Picking up item: {item.name}");
                bool isAdded = AddItemToInventory(item.itemData, item.amount);
                if (isAdded)
                {
                    //Destroy(item.gameObject);
                    photonView.RPC("DestroyItemObject", RpcTarget.All, item.gameObject.GetComponent<PhotonView>().ViewID);
                    Debug.Log($"{item.name} has been destroyed.");
                }
                else
                {
                    Debug.Log("Failed to add item to inventory");
                }
            }
        }
    }

    [PunRPC]
    private void DestroyItemObject(int networkID)
    {
        GameObject itemObject = PhotonNetwork.GetPhotonView(networkID).gameObject;
        if (itemObject != null)
        {
            PhotonNetwork.Destroy(itemObject);
        }
    }

    private bool AddItemToInventory(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            Debug.Log("itemdata prefab is null");
            return false;
        }

        foreach (Slot slot in slots)
        {
            if (slot.HasSameItem(itemData))
            {
                slot.IncreaseItemCount(amount);
                return true;
            }
        }

        foreach (Slot slot in slots)
        {
            if (!slot.HasItem)
            {
                slot.AddItem(itemData, amount);
                return true;
            }
        }
        return false;
    }

    private void DropItem(int index)
    {
        Slot slot = slots[index];
        if (slot.HasItem)
        {
            string itemName = slot.currentItem.itemName;

            if (itemName == "Wire")
            {
                GameObject engineobject_2 = GameObject.Find("Engine2");
                Enginetwo enginetwo = engineobject_2.GetComponent<Enginetwo>();

                if(engineobject_2 != null){
                    enginetwo.Interact2();
                    slot.UseItem();
                }
                else{
                    Debug.Log("engine2 못 찾음!");
                }
                /*bool interactedWithEngines = InteractWithEngines("EngineA");
                if (interactedWithEngines)
                {
                    Debug.Log("Interacted with nearby EngineA using Wire.");
                    slot.UseItem();
                }
                else
                {
                    Debug.Log("No EngineA found nearby for Wire.");
                }*/
            }
            else if (itemName == "Battery")
            {
                GameObject engineobject_3 = GameObject.Find("Engine3");
                Enginethree enginethree = engineobject_3.GetComponent<Enginethree>();

                if(engineobject_3 != null){
                    enginethree.Interact3();
                    slot.UseItem();
                }
                else{
                    Debug.Log("engine2 못 찾음!");
                }
                /*bool interactedWithEngines = InteractWithEngines("EngineB");

                if (interactedWithEngines)
                {
                    Debug.Log("Interacted with nearby EngineB using Battery.");
                }
                else
                {
                    BatteryCharge();
                    Debug.Log("Battery Charged");
                }
                slot.UseItem();*/
            }
            else
            {
                Vector3 dropPosition = player.position + player.forward * 3f;

                GameObject droppedItem = Instantiate(slot.currentItem.prefab, dropPosition, Quaternion.identity);

                ItemObject itemObject = droppedItem.GetComponent<ItemObject>();
                if (itemObject != null)
                {
                    itemObject.amount = slot.ItemCount;
                    itemObject.OnDrop();
                    slot.UseItem();
                }
            }
        }
    }

    private bool InteractWithEngines(string engineName)
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(player.position, 5f);
        bool interacted = false;

        foreach (var obj in nearbyObjects)
        {
            Engine engine = obj.GetComponent<Engine>();
            if (engine != null && engine.name == engineName)
            {
                if (engineName == "EngineA")
                {
                    engine.IncreaseBatteryCount();
                    interacted = true;
                    Debug.Log("Interacted with EngineA.");
                }
                else if (engineName == "EngineB")
                {
                    engine.IncreaseWireCount();
                    interacted = true;
                    Debug.Log("Interacted with EngineB.");
                }
            }
        }

        return interacted;
    }

    private void BatteryCharge()
    {
        if (FlashlightManager.Instance != null)
        {
            FlashlightManager.Instance.AddBatteryTime(120);
            Debug.Log($"배터리가 충전되었습니다.");
        }
        else
        {
            Debug.LogWarning("FlashlightManager를 찾을 수 없습니다.");
        }
    }
}