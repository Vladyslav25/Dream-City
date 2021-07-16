using Gameplay.Buildings;
using Gameplay.Productions;
using Gameplay.Streets;
using Gameplay.Tools;
using Grid;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        GameObject toolChoose;
        [SerializeField]
        GameObject streetType;
        [SerializeField]
        GameObject assignment;
        [SerializeField]
        GameObject production;

        private StreetTool streetTool;
        private CellAssignmentTool cellTool;
        private Outline lastOutline;
        private Image lastImage;
        private Button lastButton;

        [Header("Buttons")]
        public Button LineButton;
        public Button CurveButton;
        public Button LivingButton;
        public Button BusinessButton;
        public Button IndustryButton;

        [Header("Images")]
        public Image LivingDemand;
        public Image BusinessDemand;
        public Image IndustryDemand;
        [Header("Texts Building Info")]
        public TextMeshProUGUI AssignmentText;
        public TextMeshProUGUI DensityText;
        public TextMeshProUGUI InflowText;
        public TextMeshProUGUI Impact01Text;
        public TextMeshProUGUI Impact02Text;

        public TextMeshProUGUI MoneySum;
        public TextMeshProUGUI MoneyBalance;

        public TextMeshProUGUI StreetCostText;

        public ScrollRect ProductionListScrollRect;


        [Space]
        [SerializeField]
        private GameObject BuilingInfoObj;
        [SerializeField]
        private GameObject ProductionInfoObj;
        [SerializeField]
        private GameObject ProductionUI_Prefab;
        [SerializeField]
        private GameObject ProductionUI_Parent;
        [SerializeField]
        private GameObject SellMenuObj;
        [SerializeField]
        private GameObject StreetCostObj;

        [SerializeField]
        private ProductionQueueUI m_pqu;
        [SerializeField]
        private ProductionInfoUI m_pii;
        [SerializeField]
        private InventoryUI m_iu;
        [SerializeField]
        private SellMenuUI m_smi;

        private Dictionary<Production, ProductionBuildingUIItem> m_ProductionBuildingItems = new Dictionary<Production, ProductionBuildingUIItem>();
        [HideInInspector]
        public ProductionBuildingUIItem m_currHoveringUIItem;

        #region -SingeltonPattern-
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<UIManager>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("MeshGenerator");
                        _instance = container.AddComponent<UIManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private void Start()
        {
            streetTool = ToolManager.Instance.GetStreetTool();
            cellTool = ToolManager.Instance.GetCellAssignmentTool();
            SetActivToolChoose();
            UpdateMoneyUI();
            StreetCostObj.SetActive(false);
        }

        private void Update()
        {
            //Update ToolTip Text
            if (Instance.m_currHoveringUIItem != null)
                Instance.m_currHoveringUIItem.UpdateConditionsText();
        }

        public void SetBuildingInfoActiv(bool _active)
        {
            BuilingInfoObj.SetActive(_active);
        }

        public void SetProductionInfoActiv(bool _active)
        {
            ProductionInfoObj.SetActive(_active);
        }

        public void SetProductionInfo(ProductionBuilding _pb)
        {
            m_pii.SetProductionInfo(_pb);
        }

        public void UpdateInventoryItem(Product _p)
        {
            if (!m_iu.m_ItemProduct_Dic.ContainsKey(_p))
                m_iu.CreateItem(_p);
            m_iu.m_ItemProduct_Dic[_p].UpdateItem(_p);
        }

        public void SetBuildingStats(ProductionBuilding _b)
        {
            string assigment = "Production";
            string density = "";
            string inflowType = " Arbeitsplätze";
            string impact01Type = " Einwohner";
            string impact02Type = " Verkaufsstände";

            AssignmentText.text = assigment;
            DensityText.text = density;
            InflowText.text = _b.Inflow + inflowType;
            if (_b.Impacts[0] == 0)
            {
                Impact01Text.text = _b.Impacts[1].ToString() + impact01Type;
                Impact02Text.text = _b.Impacts[2].ToString() + impact02Type;
            }
            else if (_b.Impacts[1] == 0)
            {
                Impact01Text.text = _b.Impacts[0].ToString() + impact01Type;
                Impact02Text.text = _b.Impacts[2].ToString() + impact02Type;
            }
            else if (_b.Impacts[2] == 0)
            {
                Impact01Text.text = _b.Impacts[0].ToString() + impact01Type;
                Impact02Text.text = _b.Impacts[1].ToString() + impact02Type;
            }
        }

        public void SetBuildingStats(Building _b)
        {
            string assigment = "";
            string density = "";
            string inflowType = "";
            string impact01Type = "";
            string impact02Type = "";

            switch (_b.m_Assigment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    assigment = "Wohngebiet";
                    inflowType = " Wohnplätze";
                    impact01Type = " Verkaufsstände";
                    impact02Type = " Arbeitsplätze";
                    break;
                case EAssignment.BUSINESS:
                    assigment = "Gewerbegebiet";
                    inflowType = " Verkaufsstände";
                    impact01Type = " Einwohner";
                    impact02Type = " Arbeitsplätze";
                    break;
                case EAssignment.INDUSTRY:
                    assigment = "Industriegebiet";
                    inflowType = " Arbeitsplätze";
                    impact01Type = " Einwohner";
                    impact02Type = " Verkaufsstände";
                    break;
                default:
                    break;
            }

            switch (_b.m_Density)
            {
                case EDemand.NONE:
                    break;
                case EDemand.LOW:
                    density = "Gering";
                    break;
                case EDemand.LOWMID:
                    density = "Niedrig";
                    break;
                case EDemand.HIGHMID:
                    density = "Hoch";
                    break;
                case EDemand.HIGH:
                    density = "Sehr Hoch";
                    break;
                default:
                    break;
            }

            AssignmentText.text = assigment;
            DensityText.text = density;
            InflowText.text = _b.Inflow + inflowType;
            if (_b.Impacts[0] == 0)
            {
                Impact01Text.text = _b.Impacts[1].ToString() + impact01Type;
                Impact02Text.text = _b.Impacts[2].ToString() + impact02Type;
            }
            else if (_b.Impacts[1] == 0)
            {
                Impact01Text.text = _b.Impacts[0].ToString() + impact01Type;
                Impact02Text.text = _b.Impacts[2].ToString() + impact02Type;
            }
            else if (_b.Impacts[2] == 0)
            {
                Impact01Text.text = _b.Impacts[0].ToString() + impact01Type;
                Impact02Text.text = _b.Impacts[1].ToString() + impact02Type;
            }
        }

        public void SetDemandRatio(EAssignment _assignment, float _value)
        {
            Image img = null;
            switch (_assignment)
            {
                case EAssignment.NONE:
                    break;
                case EAssignment.LIVING:
                    img = LivingDemand;
                    break;
                case EAssignment.BUSINESS:
                    img = BusinessDemand;
                    break;
                case EAssignment.INDUSTRY:
                    img = IndustryDemand;
                    break;
            }
            if (img == null) return;
            img.fillAmount = _value;
        }

        public void SetActivToolChoose()
        {
            production.SetActive(false);
            toolChoose.SetActive(true);
            streetType.SetActive(false);
            assignment.SetActive(false);
            SellMenuObj.SetActive(false);
        }

        public void SetActivStreetType()
        {
            toolChoose.SetActive(false);
            streetType.SetActive(true);
            assignment.SetActive(false);
            production.SetActive(false);
            SellMenuObj.SetActive(false);
        }

        public void SetActivAssignment()
        {
            toolChoose.SetActive(false);
            streetType.SetActive(false);
            assignment.SetActive(true);
            production.SetActive(false);
            SellMenuObj.SetActive(false);
        }

        public void SetActivProduction()
        {
            production.SetActive(true);
        }

        public void DeactivateUI()
        {
            toolChoose.SetActive(false);
            streetType.SetActive(false);
            assignment.SetActive(false);
            SellMenuObj.SetActive(false);
        }

        public void OnClickProduction(Button _b)
        {
            SetBuildingInfoActiv(false);
            SetProductionInfoActiv(false);
            HighlightButton(_b, true);
            production.SetActive(!production.activeSelf);
        }

        public void OnClickStreetTool()
        {
            SetActivStreetType();
            ToolManager.Instance.ChangeTool(TOOLTYPE.STREET);
        }

        public void OnClickCrossTool()
        {
            DeactivateUI();
            ToolManager.Instance.ChangeTool(TOOLTYPE.CROSS);
        }

        public void OnClickAssignmentTool()
        {
            SetActivAssignment();
            ToolManager.Instance.ChangeTool(TOOLTYPE.ASSIGNMENT);
        }

        public void OnClickTeadDownTool()
        {
            DeactivateUI();
            ToolManager.Instance.ChangeTool(TOOLTYPE.TEARDOWN);
        }

        public void OnClickLine(Button sender)
        {
            streetTool.SetCurveLineTool(true);
            HighlightButton(sender);
        }

        public void OnClickCurve(Button sender)
        {
            streetTool.SetCurveLineTool(false);
            HighlightButton(sender);
        }

        public void OnClickLiving(Button sender)
        {
            cellTool.m_CurrendAssignment = EAssignment.LIVING;
            HighlightButton(sender);
        }

        public void OnClickBusiness(Button sender)
        {
            cellTool.m_CurrendAssignment = EAssignment.BUSINESS;
            HighlightButton(sender);
        }

        public void OnClickIndustry(Button sender)
        {
            cellTool.m_CurrendAssignment = EAssignment.INDUSTRY;
            HighlightButton(sender);
        }

        public void OnClickClear(Button sender)
        {
            cellTool.m_CurrendAssignment = EAssignment.NONE;
            HighlightButton(sender);
        }

        public void IncreaseOutline(Outline _o, bool _canBeToggle)
        {
            ResetOutline(lastOutline);

            if (lastOutline == _o && _canBeToggle) return;

            _o.effectDistance = new Vector2(3, 3);
            lastOutline = _o;
        }

        public void OpenInventorySellUI(InventoryUIItem _iui)
        {
            SellMenuObj.SetActive(true);
            m_smi.UpdateInfo(_iui.m_Product);
        }

        public void UpdateInventorySellUI(Product _p)
        {
            m_smi.UpdateInfo(_p);
        }

        public void UpdateCurrendProductSellUI(Product _p)
        {
            if (_p == m_smi.m_currProduct)
            {
                m_smi.UpdateInfo(_p);
            }
        }

        public void CloseInventorySellUI()
        {
            SellMenuObj.SetActive(false);
        }

        private void ResetOutline(Outline _o)
        {
            if (_o != null)
                _o.effectDistance = new Vector2(1, 1);
        }

        /// <summary>
        /// Highlight a Button with Color and Outline. Also remove the Highlight of the last Button
        /// </summary>
        /// <param name="_b">The Button to Highlight</param>
        public void HighlightButton(Button _b, bool _isToggel = false)
        {
            if (_b == null) return;

            Image i = _b.GetComponent<Image>();
            if (!_isToggel)
                ResetHighlightButton();
            i.color = Color.white;

            lastImage = i;

            Outline o = _b.GetComponent<Outline>();
            if (o != null)
                IncreaseOutline(o, _isToggel);
        }

        public void ResetHighlightButton()
        {
            if (lastImage != null && lastImage.gameObject.tag != "ToggleButton")
                lastImage.color = new Color(0.77f, 0.77f, 0.77f, 1);
        }

        public void InitProductionUI(ProductionBuilding _pb)
        {
            GameObject obj = Instantiate(ProductionUI_Prefab, ProductionUI_Parent.transform);
            ProductionBuildingUIItem pbUI = obj.GetComponent<ProductionBuildingUIItem>();
            pbUI.Init(_pb);
            if (!m_ProductionBuildingItems.ContainsKey(_pb.m_Production))
                m_ProductionBuildingItems.Add(_pb.m_Production, pbUI);
        }

        public void MoveScrollRectContentToTop()
        {
            //need to set active to move the rect
            ProductionListScrollRect.transform.gameObject.SetActive(true);
            ProductionListScrollRect.verticalNormalizedPosition = 1;
            ProductionListScrollRect.transform.gameObject.SetActive(false);
        }

        public void AddProductionBuildingItem(ProductionBuilding _pb)
        {
            m_pqu.AddToQueue(_pb);
        }

        public void ChangeProductionItemColor(Color _color, int _index = 0)
        {
            m_pqu.ChangeColor(_index, _color);
        }

        public void RemoveProductionBuildingItem(int _index)
        {
            m_pqu.RemoveItem(_index);
        }

        public void UpdateMoneyUI()
        {
            MoneySum.text = ConvertFloatToStringPrice(Inventory.Instance.m_MoneyAmount);
            MoneyBalance.text = ConvertFloatToStringPriceWithSign(Inventory.Instance.m_MoneyBalance, out Color c);
            MoneyBalance.color = c;
        }

        public void UnlockProduction(Production _production)
        {
            m_ProductionBuildingItems[_production].UnlockProduction();
        }

        public void UpdateConditionText(Production _production)
        {
            m_ProductionBuildingItems[_production].UpdateConditionsText();
        }

        public void SetStreetCost(float _value)
        {
            StreetCostObj.SetActive(true);
            StreetCostText.text = ConvertFloatToStringPriceWithSign(_value, out Color c);
            StreetCostText.color = c;
        }

        public void SetStreetCostActive(bool _active)
        {
            StreetCostObj.SetActive(_active);
        }

        public void ChangeInventoryUIItemBGColor(Product _p, Color _color)
        {
            if (m_iu.m_ItemProduct_Dic.ContainsKey(_p))
                m_iu.m_ItemProduct_Dic[_p].ChangeBGColor(_color);
        }

        #region -Static-
        public static string ConvertFloatToStringDigit(float _input)
        {
            return string.Format("{0:0}", _input);
        }

        public static string ConvertFloatToStringDigit(float _input, out Color c)
        {
            c = Color.green;

            if (_input > 0)
                c = Color.green;
            else if (_input == 0)
                c = Color.yellow;
            else
                c = Color.red;

            return string.Format("{0:0}", _input);
        }

        public static string ConvertFloatToStringDigitWithSign(float _input, out Color c)
        {
            string s = string.Format("{0:0.00}", _input);
            string sign = "";
            c = Color.green;

            if (_input > 0)
            {
                sign = "+ ";
                c = Color.green;
            }
            else if (_input == 0)
                c = Color.yellow;
            else
                c = Color.red;

            if (s.EndsWith("00"))
            {
                return sign + ((int)_input).ToString();
            }
            else
            {
                return sign + s;
            }
        }

        public static string ConvertFloatToStringDecimal(float _input)
        {
            return string.Format("{0:0.00}", _input);
        }

        public static string ConvertFloatToStringPrice(float _input)
        {
            string s = string.Format("{0:0.00}", _input);

            if (s.EndsWith("00"))
            {
                return ((int)_input).ToString() + " €";
            }
            else
            {
                return s + " €";
            }
        }

        public static string ConvertFloatToStringPriceWithSign(float _input)
        {
            string s = string.Format("{0:0.00}", _input);
            string sign = "";

            if (_input > 0)
                sign = "+ ";

            if (s.EndsWith("00"))
            {
                return sign + ((int)_input).ToString() + " €";
            }
            else
            {
                return sign + s + " €";
            }
        }

        public static string ConvertFloatToStringPriceWithSign(float _input, out Color c)
        {
            string s = string.Format("{0:0.00}", _input);
            string sign = "";
            c = Color.green;

            if (_input > 0)
            {
                sign = "+ ";
                c = Color.green;
            }
            else if (_input == 0)
                c = Color.yellow;
            else
                c = Color.red;

            if (s.EndsWith("00"))
            {
                return sign + ((int)_input).ToString() + " €";
            }
            else
            {
                return sign + s + " €";
            }
        }

        public static float ConvertStringToFloat(string _input)
        {
            if (_input.Length == 0) return 0f;
            if (float.TryParse(_input, out float result))
                return result;
            Debug.LogError("Wrong Input. Input is: " + _input);
            return 0f;
        }
        #endregion
    }
}
