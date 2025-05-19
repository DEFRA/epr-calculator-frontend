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
        private string responseMessage;
        private string name;
        private EditContext? editContext;

        protected override async Task OnInitializedAsync()
        {
            this.OrgProducers = new List<OrgProducerData>();
            var count = 7000;

            for (int i = 1; i < count; i++)
            {
                this.OrgProducers.Add(new OrgProducerData() { OrganisationName = "Acme org Ltd", OrganisationID = i.ToString(), BillingInstructions = "DELTA", InvoiceAmount = "£100.000", Status = "ACCEPTED" });
            }

            this.OrgProducers.Add(new OrgProducerData() { OrganisationName = "Acme org Ltd", OrganisationID = (count + 1).ToString(), BillingInstructions = "CANCEL BILL", InvoiceAmount = "£100.000", Status = "REJECTED" });
            this.OrgProducers.Add(new OrgProducerData() { OrganisationName = "Acme org Ltd", OrganisationID = (count + 2).ToString(), BillingInstructions = "INITIAL", InvoiceAmount = "£100.000", Status = "PENDING" });
        }

        public class ElementComparer : IEqualityComparer<OrgProducerData>
        {
            public bool Equals(OrgProducerData a, OrgProducerData b) => a?.OrganisationID == b?.OrganisationID;

            public int GetHashCode(OrgProducerData x) => HashCode.Combine(x?.OrganisationID);
        }

        private IEnumerable<OrgProducerData> GetVisiblePageItems()
        {
            return this._orgtableRef.FilteredItems.Skip(this._orgtableRef.CurrentPage * _orgtableRef.RowsPerPage).Take(_orgtableRef.RowsPerPage);
        }

        private void SelectPage()
        {
            if (this._orgtableRef == null)
            {
                return;
            }

            var pageItems = this.GetVisiblePageItems();
            foreach (var item in pageItems)
            {
                this.selectedItems.Add(item);
            }
        }

        private void DeselectPage()
        {
            if (this._orgtableRef == null)
            {
                return;
            }

            var pageItems = this.GetVisiblePageItems();
            foreach (var item in pageItems)
            {
                this.selectedItems.Remove(item);
            }
        }

        private bool? PageSelectionState
        {
            get
            {
                if (this._orgtableRef == null)
                {
                    return false;
                }

                var pageItems = this.GetVisiblePageItems();
                if (pageItems.All(item => this.selectedItems.Contains(item)))
                {
                    return true; // All items are selected
                }

                if (pageItems.Any(item => this.selectedItems.Contains(item)))
                {
                    return null; // Some items are selected (indeterminate)
                }

                return false; // No items are selected
            }

            set
            {
                if (value == true || value == null)
                {
                    this.SelectPage();
                }
                else if (value == false)
                {
                    this.DeselectPage();
                }
            }
        }

        private async Task HandleSubmit()
        {
            Console.WriteLine(this.name);
            var response = await this.Http.PostAsJsonAsync("blazoreGrid/submit", this.selectedItems.ToList());
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                this.responseMessage = json;
            }
            else
            {
                this.responseMessage = "Form failed to Submit";
            }

            Console.WriteLine(this.responseMessage);
        }

        private void HandleReset()
        {
            this.selectedItems.Clear();
        }

        public record OrgProducerData
        {
            public string OrganisationName { get; set; }

            public string OrganisationID { get; set; }

            public string BillingInstructions { get; set; }

            public string InvoiceAmount { get; set; }

            public string Status { get; set; }
        }
    }
}
