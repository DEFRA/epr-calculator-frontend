using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace EPR.Calculator.Frontend.Components
{
    public partial class Grid
    {
        private bool _hidePosition;
        private List<OrgProducerData> OrgProducers = new List<OrgProducerData>();
        private HashSet<OrgProducerData> selectedItems = new HashSet<OrgProducerData>();
        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Right;
        private bool hidePageNumber;
        private bool hidePagination;
        private bool hideRowsPerPage;
        private string rowsPerPageString = "Rows per page:";
        private string infoFormat = "{first_item}-{last_item} of {all_items}";
        private string allItemsText = "All";
        private MudTable<OrgProducerData> _orgtableRef;
        private MudForm form;
        private string ResponseMessage;
        private string name;
        private EditContext? editContext;

        protected override async Task OnInitializedAsync()
        {
            OrgProducers = new List<OrgProducerData>();
            var count = 7000;

            for (int i = 1; i < count; i++)
            {
                OrgProducers.Add(new OrgProducerData() { OrganisationName = "Acme org Ltd", OrganisationID = i.ToString(), BillingInstructions = "DELTA", InvoiceAmount = "£100.000", Status = "ACCEPTED" });
            }

            OrgProducers.Add(new OrgProducerData() { OrganisationName = "Acme org Ltd", OrganisationID = (count + 1).ToString(), BillingInstructions = "CANCEL BILL", InvoiceAmount = "£100.000", Status = "REJECTED" });
            OrgProducers.Add(new OrgProducerData() { OrganisationName = "Acme org Ltd", OrganisationID = (count + 2).ToString(), BillingInstructions = "INITIAL", InvoiceAmount = "£100.000", Status = "PENDING" });
        }

        class ElementComparer : IEqualityComparer<OrgProducerData>
        {
            public bool Equals(OrgProducerData a, OrgProducerData b) => a?.OrganisationID == b?.OrganisationID;

            public int GetHashCode(OrgProducerData x) => HashCode.Combine(x?.OrganisationID);
        }

        public record OrgProducerData
        {
            public string OrganisationName { get; set; }

            public string OrganisationID { get; set; }

            public string BillingInstructions { get; set; }

            public string InvoiceAmount { get; set; }

            public string Status { get; set; }
        }

        private IEnumerable<OrgProducerData> GetVisiblePageItems()
        {
            return _orgtableRef.FilteredItems.Skip(_orgtableRef.CurrentPage * _orgtableRef.RowsPerPage).Take(_orgtableRef.RowsPerPage);
        }

        private void SelectPage()
        {
            if (_orgtableRef == null) return;
            var pageItems = GetVisiblePageItems();
            foreach (var item in pageItems)
                selectedItems.Add(item);
        }

        private void DeselectPage()
        {
            if (_orgtableRef == null) return;
            var pageItems = GetVisiblePageItems();
            foreach (var item in pageItems)
                selectedItems.Remove(item);
        }

        private bool? PageSelectionState
        {
            get
            {
                if (_orgtableRef == null) return false;
                var pageItems = GetVisiblePageItems();
                if (pageItems.All(item => selectedItems.Contains(item)))
                    return true; // All items are selected
                if (pageItems.Any(item => selectedItems.Contains(item)))
                    return null; // Some items are selected (indeterminate)
                return false; // No items are selected
            }

            set
            {
                if (value == true || value == null)
                {
                    SelectPage();
                }
                else if (value == false)
                {
                    DeselectPage();
                }
            }
        }

        private async Task HandleSubmit()
        {
            Console.WriteLine(name);
            var response = await Http.PostAsJsonAsync("blazoreGrid/submit", selectedItems.ToList());
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                ResponseMessage = json;
            }
            else
            {
                ResponseMessage = "Form failed to Submit";
            }

            Console.WriteLine(ResponseMessage);
        }

        private async Task HandleFailure()
        {
            Console.WriteLine("failed");
        }

        private async Task HandleReset()
        {
            selectedItems.Clear();
        }
    }
}
