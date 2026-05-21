using mowt.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Layout
{
    public class NavigationService
    {
        private readonly IConfiguration _config;
        private readonly IFormFactor _formFactor;
        private bool isWeb;

        public NavigationService(IConfiguration config, IFormFactor formFactor)
        {
            _config = config;
            _formFactor = formFactor;
            isWeb = _formFactor.GetPlatform().Contains("web");
        }
        public List<NavItem> GetAllNavigationItems()
        {
            return new List<NavItem>
            {
                // Dashboard
                new NavItem("Dashboard", "admin/dashboard", "", new Icons.Regular.Size20.Home(), true),
                
                // Documents group
                new NavItem("Categories", "admin/documents/categories", "Documents", new Icons.Regular.Size20.Group()),
                new NavItem("Segments", "admin/documents/segments", "Documents", new Icons.Regular.Size20.GridDots()),
                new NavItem("Suppliers", "admin/documents/suppliers", "Documents", new Icons.Regular.Size20.ContactCardGroup()),
                new NavItem("Document setup", "admin/documents", "Documents", new Icons.Regular.Size20.BookAdd(), true),
                new NavItem("Feedback", "admin/documents/feedback-review", "Documents", new Icons.Regular.Size20.Feed(), true),
                //new NavItem("Inventory Management", "stockcount", "Documents", new Icons.Regular.Size20.BoxMultipleCheckmark()),
                //new NavItem("ReceiveStock (GRN)", "documents/ReceiveProducts_GRN_", "Documents", new Icons.Regular.Size20.Receipt()),
                //new NavItem("Barcode Generator", "documents/barcode-generator", "Documents", new Icons.Regular.Size20.BarcodeScanner()),
                //new NavItem("Product Import/Export", "import-export/documents", "documents > Imports/Exports", new Icons.Regular.Size20.ArrowImport()),
                //new NavItem("Category Import/Export", "import-export/categories", "documents > Imports/Exports", new Icons.Regular.Size20.ArrowAutofitWidthDotted()),
                //new NavItem("Segments Import/Export", "import-export/segments", "documents > Imports/Exports", new Icons.Regular.Size20.ArrowBidirectionalLeftRight()),
                
                // Customers group
                //new NavItem("Customer Setup", "customers", "Customers", new Icons.Regular.Size20.PeopleAdd()),
                //new NavItem("Customer based pricing", "customer-pricing", "Customers", new Icons.Regular.Size20.CoinStack()),
                //new NavItem("Customer Accounts", "customer/accounts", "Customers", new Icons.Regular.Size20.PersonAccounts()),
                //new NavItem("Customer Import/Export", "import-export/customers", "Customers", new Icons.Regular.Size20.ArrowDownload()),
                
                // Suppliers group
                //new NavItem("Supplier Setup", "suppliers", "Suppliers", new Icons.Regular.Size20.ContactCardGroup()),
                //new NavItem("Supplier Accounts", "supplier/accounts", "Suppliers", new Icons.Regular.Size20.PersonAccounts()),
                //new NavItem("Supplier Import/Export", "import-export/supplier", "Suppliers", new Icons.Regular.Size20.PersonAccounts()),
                
                // History group
                new NavItem("Reprint Receipts", "counter", "History", new Icons.Regular.Size20.ReceiptPlay()),
                new NavItem("System Log", "weather", "History", new Icons.Regular.Size20.System()),
                
                // Reports group
                new NavItem("Daily Sales Report", "admin/reports/daily-sales", "Reports > Sales", new Icons.Regular.Size20.DataBarVerticalAdd()),
                new NavItem("Sales Per Product", "admin/reports/sales-per-product", "Reports > Sales", new Icons.Regular.Size20.BackpackAdd()),
                new NavItem("Detailed Sales Log", "admin/reports/detailed-sales-log", "Reports > Sales", new Icons.Regular.Size20.ClipboardBulletList()),
                new NavItem("Sales Per Customer", "admin/reports/sales-per-customer", "Reports > Sales", new Icons.Regular.Size20.PeopleList()),
                new NavItem("Shift Performance", "admin/reports/shift-performance", "Reports > Sales", new Icons.Regular.Size20.PersonMoney()),
                new NavItem("Sales Per Category and Segment", "admin/reports/sales-per-category-and-segment", "Reports > Sales", new Icons.Regular.Size20.DocumentBulletList()),
                new NavItem("Stock Movement Report", "admin/inventory/stock-movement", "Reports > Inventory", new Icons.Regular.Size20.Shifts()),
                new NavItem("Product Purchases", "admin/inventory/product-purchases", "Reports > Inventory", new Icons.Regular.Size20.Backpack()),
                new NavItem("Account Statement Per Customer", "admin/reports/customer-account-statement", "Reports > Customers", new Icons.Regular.Size20.Person()),
                new NavItem("Supplier Statement", "admin/reports/supplier-statement", "Reports > Suppliers", new Icons.Regular.Size20.PersonSettings()),
                //new NavItem("General Expenses", "reports/general-expenses", "Reports > Expenses", new Icons.Regular.Size20.TrayItemRemove()),
                
                // Settings group
                new NavItem("User Accounts", "admin/settings/users", "Settings", new Icons.Regular.Size20.PeopleAdd()),
                new NavItem("Subscriptions", "admin/settings/subscriptions", "Settings", new Icons.Regular.Size20.BuildingMultiple()),
                //new NavItem("Shift Manager", "shifts", "Settings", new Icons.Regular.Size20.Shifts()),
                //new NavItem("Receipt Slip design", "admin/settings/canvas", "Settings", new Icons.Regular.Size20.ReceiptSearch()),
                new NavItem("Backup Database", "admin/dashboard1?isbackuprequest=true", "Settings > Backup and Restore", new Icons.Regular.Size20.Backpack()),
                new NavItem("Restore from backup", "admin/dashboard2?isrestorerequest=true", "Settings > Backup and Restore", new Icons.Regular.Size20.DatabaseArrowDown()),
                //new NavItem("Reset and Delete Data", "dashboard3?isdeletedatarequest=true", "Settings > Backup and Restore", new Icons.Regular.Size20.Delete()),
                new NavItem("Configurations", "admin/settings/configurations", "Settings", new Icons.Regular.Size20.WrenchSettings())
            };
        }

        public List<NavGroup> BuildNavigationStructure()
        {
            var allNavItems = GetAllNavigationItems();
            var rootGroups = new List<NavGroup>();
            var groupMap = new Dictionary<string, NavGroup>();


            var rootNavItems = new Dictionary<int, NavItem>();
            var rootItemPositions = new Dictionary<string, int>();
            if (isWeb || _config["BackendConfig:Mode"] != "1")
            {
                allNavItems.RemoveAll(item => item.GroupPath.ToLower().Contains("settings > backup and restore"));
            }

            int currentPosition = 0;

            foreach (var item in allNavItems)
            {
                if (string.IsNullOrEmpty(item.GroupPath))
                {
                    // This is a standalone item like Dashboard
                    rootNavItems[currentPosition] = item;
                    rootItemPositions[item.Title] = currentPosition;
                }
                else
                {
                    var groupParts = item.GroupPath.Split(new[] { " > " }, StringSplitOptions.RemoveEmptyEntries);
                    var rootGroupName = groupParts[0];

                    // Track the first position where we see each root group
                    if (!rootItemPositions.ContainsKey(rootGroupName))
                    {
                        rootItemPositions[rootGroupName] = currentPosition;
                    }
                }

                currentPosition++;
            }

            // First, create all group objects
            foreach (var item in allNavItems)
            {
                if (string.IsNullOrEmpty(item.GroupPath))
                    continue;

                var groupParts = item.GroupPath.Split(new[] { " > " }, StringSplitOptions.RemoveEmptyEntries);
                var currentPath = "";
                NavGroup? parentGroup = null;

                foreach (var part in groupParts)
                {
                    currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath} > {part}";

                    if (!groupMap.TryGetValue(currentPath, out var group))
                    {
                        // Create a new group
                        group = new NavGroup(part)
                        {
                            ParentPath = parentGroup?.FullPath ?? ""
                        };

                        groupMap[currentPath] = group;

                        if (parentGroup != null)
                        {
                            // This is a subgroup
                            parentGroup.SubGroups.Add(group);
                        }
                    }

                    parentGroup = group;
                }
            }

            // Set icons for root groups
            SetRootGroupIcons(groupMap);

            foreach (var item in allNavItems)
            {
                if (!string.IsNullOrEmpty(item.GroupPath))
                {
                    // Add to the appropriate group
                    if (groupMap.TryGetValue(item.GroupPath, out var group))
                    {
                        group.Items.Add(item);
                    }
                }
            }
            var sortedPositions = rootItemPositions.OrderBy(kvp => kvp.Value).ToList();

            foreach (var position in sortedPositions)
            {
                string itemName = position.Key;

                if (rootNavItems.Values.Any(item => item.Title == itemName))
                {
                    // This is a standalone item
                    var item = rootNavItems.Values.First(i => i.Title == itemName);
                    var standAloneGroup = new NavGroup(item.Title)
                    {
                        Icon = item.Icon
                    };
                    standAloneGroup.Items.Add(item);
                    rootGroups.Add(standAloneGroup);
                }
                else if (groupMap.ContainsKey(itemName))
                {
                    // This is a group
                    rootGroups.Add(groupMap[itemName]);
                }
            }

            return rootGroups;
        }

        private void SetRootGroupIcons(Dictionary<string, NavGroup> groupMap)
        {
            // Set icons for each root group
            if (groupMap.TryGetValue("Documents", out var documentsGroup))
                documentsGroup.Icon = new Icons.Regular.Size20.Book();

            if (groupMap.TryGetValue("Customers", out var customersGroup))
                customersGroup.Icon = new Icons.Regular.Size20.PeopleStar();

            if (groupMap.TryGetValue("Suppliers", out var suppliersGroup))
                suppliersGroup.Icon = new Icons.Regular.Size20.PeopleCall();

            if (groupMap.TryGetValue("History", out var historyGroup))
                historyGroup.Icon = new Icons.Regular.Size20.BuildingFactory();

            if (groupMap.TryGetValue("Reports", out var reportsGroup))
                reportsGroup.Icon = new Icons.Regular.Size20.CardUi();

            if (groupMap.TryGetValue("Reports > Sales", out var salesGroup))
                salesGroup.Icon = new Icons.Regular.Size20.CoinStack();

            if (groupMap.TryGetValue("Reports > Inventory", out var inventoryGroup))
                inventoryGroup.Icon = new Icons.Regular.Size20.BoxMultipleCheckmark();

            if (groupMap.TryGetValue("Reports > Customers", out var customerReportsGroup))
                customerReportsGroup.Icon = new Icons.Regular.Size20.ContactCardGroup();

            if (groupMap.TryGetValue("Reports > Suppliers", out var supplierReportsGroup))
                supplierReportsGroup.Icon = new Icons.Regular.Size20.PersonKey();

            if (groupMap.TryGetValue("Reports > Expenses", out var expensesGroup))
                expensesGroup.Icon = new Icons.Regular.Size20.MoneyOff();

            if (groupMap.TryGetValue("Settings", out var settingsGroup))
                settingsGroup.Icon = new Icons.Regular.Size20.Settings();

            if (groupMap.TryGetValue("Documents > Imports/Exports", out var productImportsGroup))
                productImportsGroup.Icon = new Icons.Regular.Size20.ArrowDownload();

            if (groupMap.TryGetValue("Settings > Backup and Restore", out var backupRestoreGroup))
                backupRestoreGroup.Icon = new Icons.Regular.Size20.DatabaseLightning();

            if (groupMap.TryGetValue("Reports > Sales > Imports/Exports", out var salesImportsGroup))
                salesImportsGroup.Icon = new Icons.Regular.Size20.ArrowDownload();
        }

    }
}
