using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [SerializeField] public Camera minimapCamera;
    [SerializeField] public Transform navArrow;
    [SerializeField] public GameObject iconPrefab;
    [SerializeField] public Transform iconsParent;
    [SerializeField] public Sprite campfireSprite;
    // Set the icons not to go all the way to the edge
    [SerializeField] public float borderPadding = 11.0f;

    protected bool hasInstantiatedIcons = false;
    private Transform playerBody;
    // List of pairs of the actual station and the icon of that station
    private Dictionary<int, (Transform, Transform)> orderSubmissionStationLocationsAndIcons;
    // List of campfire icons
    private List<(Transform, Transform)> campfireIcons = new List<(Transform, Transform)>();
    protected float width;
    protected float height;
    private Vector3 playerOldPosition;

    // Ensures that icons are set when player enters the scene
    private bool firstSettingOfIconPositions = false;

    void Awake()
    {
        playerBody = GameObject.FindGameObjectWithTag("PlayerBody").transform;
    }

    private void InstantiateCampfireIcons()
    {
        campfireIcons = new List<(Transform, Transform)>();
        List<Transform> listOfCampfireLocations = RecipeManager.instance.GetAllCampfires();
        foreach (Transform fire in listOfCampfireLocations)
        {
            GameObject icon = Instantiate(iconPrefab,
                                          new Vector3(0, 0, 0),
                                          Quaternion.identity,
                                          iconsParent.transform) as GameObject;
            // Set icon image
            icon.GetComponent<Image>().sprite = campfireSprite;
            campfireIcons.Add((fire, icon.transform));
        }
    }

    protected void InstantiateMinimapIcons()
    {
        if (hasInstantiatedIcons)
        {
            return;
        }

        foreach (Transform child in iconsParent)
        {
            Destroy(child.gameObject);
        }
        
        width = GetMapWidth();
        height = GetMapHeight();
        
        // Add campfires
        InstantiateCampfireIcons();

        // Add order icons
        orderSubmissionStationLocationsAndIcons = new Dictionary<int, (Transform, Transform)>();
        playerOldPosition = playerBody.position;

        Dictionary<int, (Transform, Sprite)> relevantOrders = OrdersManager.instance.GetRelevantOrderStations();
        foreach (KeyValuePair<int, (Transform, Sprite)> order in relevantOrders)
        {
            int stationId = order.Key;
            Transform stationTransform = order.Value.Item1;
            GameObject minimapIcon = Instantiate(iconPrefab,
                                                 new Vector3(0, 0, 0),
                                                 Quaternion.identity,
                                                 iconsParent.transform) as GameObject;
            // Set icon image
            minimapIcon.GetComponent<Image>().sprite = order.Value.Item2;
            orderSubmissionStationLocationsAndIcons.Add(stationId, (stationTransform, minimapIcon.transform));
        }

        // Register the callbacks
        OrdersManager.instance.AddOrderCompletionCallback((stationId, _) =>
        {
            if (orderSubmissionStationLocationsAndIcons.ContainsKey(stationId))
            {
                Destroy(orderSubmissionStationLocationsAndIcons[stationId].Item2.gameObject);
                orderSubmissionStationLocationsAndIcons.Remove(stationId);
            }
        });
        OrdersManager.instance.AddOrderGenerationCallback(() => ResetInstantiatedOrderIconsFlag());
        hasInstantiatedIcons = true;
    }

    // Calling this will trigger the minimap to redraw the icons
    private void ResetInstantiatedOrderIconsFlag()
    {
        hasInstantiatedIcons = false;
    }

    public virtual void Update()
    {
        if (OrdersManager.instance.IsOrderGenerationComplete() && !hasInstantiatedIcons)
        {
            InstantiateMinimapIcons();
            return;
        }
        else if (!hasInstantiatedIcons)
        {
            return;
        }
        CheckIfPlayerHasMoved();   
    }

    // Check if player has moved and perform the relevant updates
    protected void CheckIfPlayerHasMoved()
    {
        if (playerOldPosition != playerBody.position || !firstSettingOfIconPositions)
        {
            playerOldPosition = playerBody.position;
            // Update positions of old icons
            foreach ((Transform station, Transform icon) in orderSubmissionStationLocationsAndIcons.Values)
            {
                SetIconPos(station, icon, false);
            }
            foreach ((Transform fire, Transform icon) in campfireIcons)
            {
                SetIconPos(fire, icon, true);
            }
            navArrow.eulerAngles = new Vector3(0, 0, -playerBody.eulerAngles.y);
            firstSettingOfIconPositions = true;
        }
    }

    // Gets the width of the minimap
    protected virtual float GetMapWidth()
    {
        RectTransform rt = this.GetComponent<RectTransform>();
        return rt.sizeDelta.x;
    }

    // Gets the height of the minimap
    protected virtual float GetMapHeight()
    {
        RectTransform rt = this.GetComponent<RectTransform>();
        return rt.sizeDelta.y;
    }

    private void SetIconPos(Transform target, Transform icon, bool hideIfFarAway)
    {
        if (target == null)
        {
            icon.gameObject.SetActive(false);
            return;
        }

        icon.gameObject.SetActive(true);
        Vector3 screenPos = minimapCamera.WorldToScreenPoint(target.position) -
                            minimapCamera.WorldToScreenPoint(playerOldPosition);
        Vector3 localPos = new Vector3(screenPos.x,
                                       screenPos.y,
                                       0);

        float halfWidth = width / 2 - borderPadding;
        float halfHeight = height / 2 - borderPadding;
        bool exceedX = Mathf.Abs(localPos.x) > halfWidth;
        bool exceedY = Mathf.Abs(localPos.y) > halfHeight;

        if ((exceedX || exceedY) && hideIfFarAway)
        {
            icon.gameObject.SetActive(false);
            return;
        }

        if (exceedX)
        {
            localPos.x = Mathf.Sign(localPos.x) * halfWidth;
        }
        if (exceedY)
        {
            localPos.y = Mathf.Sign(localPos.y) * halfHeight;
        }

        icon.GetComponent<RectTransform>().anchoredPosition = localPos;
    }
}
