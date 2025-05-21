using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;        // 物品名称
        public int price;             // 物品价格
        public GameObject priceObject; // 价格显示对象
        public GameObject toggleObject; // 开关显示对象
        public TMP_Text priceText;     // 价格文本组件
        public Button priceButton;     // 价格按钮组件
    }

    [Header("UI References")]
    public TMP_Text gemCountText;     // 宝石数量显示文本
    public List<ShopItem> shopItems;  // 商店物品列表

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;        // 调试模式开关
    [SerializeField] private bool resetOnStart = false;     // 启动时重置所有物品状态
    [SerializeField] private int debugGemAmount = 1000;     // 调试用宝石数量

    private const string GEM_COUNT_KEY = "GemCount"; // 用于保存宝石数量的键
    private const string ITEM_PURCHASED_KEY = "ItemPurchased_"; // 用于保存物品购买状态的键前缀

    private void Start()
    {
        if (debugMode)
        {
            if (resetOnStart)
            {
                ResetAllItems();
            }
            SetGemAmount(debugGemAmount);
        }
        
        InitializeShop();
        UpdateGemCountDisplay();
    }

    // 重置所有物品的购买状态
    private void ResetAllItems()
    {
        foreach (var item in shopItems)
        {
            // 清除物品购买状态
            PlayerPrefs.DeleteKey(ITEM_PURCHASED_KEY + item.itemName);
            // 更新显示
            UpdateItemDisplay(item, false);
        }
        PlayerPrefs.Save();
        Debug.Log("已重置所有物品的购买状态");
    }

    // 设置宝石数量
    private void SetGemAmount(int amount)
    {
        PlayerPrefs.SetInt(GEM_COUNT_KEY, amount);
        PlayerPrefs.Save();
        UpdateGemCountDisplay();
        Debug.Log($"已设置宝石数量为: {amount}");
    }

    private void InitializeShop()
    {
        foreach (var item in shopItems)
        {
            // 设置价格文本
            if (item.priceText != null)
            {
                item.priceText.text = item.price.ToString();
            }

            // 设置按钮点击事件
            if (item.priceButton != null)
            {
                item.priceButton.onClick.AddListener(() => OnItemClick(item));
            }

            // 检查物品是否已购买
            bool isPurchased = PlayerPrefs.GetInt(ITEM_PURCHASED_KEY + item.itemName, 0) == 1;
            UpdateItemDisplay(item, isPurchased);
        }
    }

    private void OnItemClick(ShopItem item)
    {
        int currentGems = PlayerPrefs.GetInt(GEM_COUNT_KEY, 0);

        if (currentGems >= item.price)
        {
            // 扣除宝石
            currentGems -= item.price;
            PlayerPrefs.SetInt(GEM_COUNT_KEY, currentGems);
            PlayerPrefs.Save();

            // 标记物品为已购买
            PlayerPrefs.SetInt(ITEM_PURCHASED_KEY + item.itemName, 1);
            PlayerPrefs.Save();

            // 更新显示
            UpdateGemCountDisplay();
            UpdateItemDisplay(item, true);

            Debug.Log($"成功购买物品: {item.itemName}");
        }
        else
        {
            Debug.Log("宝石不足，无法购买");
        }
    }

    private void UpdateItemDisplay(ShopItem item, bool isPurchased)
    {
        if (item.priceObject != null)
        {
            item.priceObject.SetActive(!isPurchased);
        }
        if (item.toggleObject != null)
        {
            item.toggleObject.SetActive(isPurchased);
        }
    }

    private void UpdateGemCountDisplay()
    {
        if (gemCountText != null)
        {
            int currentGems = PlayerPrefs.GetInt(GEM_COUNT_KEY, 0);
            gemCountText.text = currentGems.ToString();
        }
    }
} 