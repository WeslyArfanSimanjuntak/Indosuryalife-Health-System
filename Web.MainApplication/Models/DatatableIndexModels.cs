using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Web.MainApplication.Models
{
    public class DatatableIndexModels
    {
        private string _url;
        private List<ColumnsDetail> _listOfColumns;
        private string _tableId;
        private string _filterClearButtonId;
        private string _filterButtonId;
        private string _filterString;
        private bool _footerShow = false;
        private bool _viewShow = true;
        private bool _editShow = true;
        private List<string> _columnHeader;
        private string _iconTreeName = "";
        private List<ColumnsDetail> _showLink = null;
        private List<ColumnsDetail> _order = null;

        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                _url = value;
            }
        }
        public List<ColumnsDetail> ListOfColumns
        {
            get
            {
                return _listOfColumns;
            }

            set
            {
                _listOfColumns = value;
            }
        }
        public string TableId
        {
            get
            {
                return _tableId;
            }

            set
            {
                _tableId = value;
            }
        }
        public string FilterClearButtonId
        {
            get
            {
                return _filterClearButtonId;
            }

            set
            {
                _filterClearButtonId = value;
            }
        }
        public string FilterButtonId
        {
            get
            {
                return _filterButtonId;
            }

            set
            {
                _filterButtonId = value;
            }
        }
        public string FilterString
        {
            get
            {
                return _filterString;
            }

            set
            {
                _filterString = value;
            }
        }
        public bool FooterShow
        {
            get
            {
                return _footerShow;
            }

            set
            {
                _footerShow = value;
            }
        }
        public bool ViewShow
        {
            get
            {
                return _viewShow;
            }

            set
            {
                _viewShow = value;
            }
        }
        public bool EditShow
        {
            get
            {
                return _editShow;
            }

            set
            {
                _editShow = value;
            }
        }
        public List<string> ColumnHeader
        {
            get
            {
                return _columnHeader;
            }

            set
            {
                _columnHeader = value;
            }
        }
        public string IconTreeName
        {
            get
            {
                return _iconTreeName;
            }

            set
            {
                _iconTreeName = value;
            }
        }
        public List<ColumnsDetail> ShowLink
        {
            get
            {
                return _showLink;
            }

            set
            {
                _showLink = value;
            }
        }
        public List<ColumnsDetail> Order
        {
            get
            {
                return _order;
            }

            set
            {
                _order = value;
            }
        }

        public DatatableIndexModels(Type entitity, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, bool footerShow = false, bool viewShow = true, bool editShow = true)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();

            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = pInfo.Name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });
            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            _footerShow = footerShow;
            _viewShow = viewShow;
            _editShow = editShow;
            _columnHeader = columnHeader.ToList();
        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {

                    string name = (columnNameToChange != null && (columnNameToChange.ContainsKey(pInfo.Name))) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }
        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, bool footerShow = false)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    if (columnNameToChange != null)
                    {
                        string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                        listOfColumn.Add(new ColumnsDetail
                        {
                            Name = name,
                            Data = pInfo.Name,
                            Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                        });
                    }
                    else
                    {
                        listOfColumn.Add(new ColumnsDetail
                        {
                            Name = pInfo.Name,
                            Data = pInfo.Name,
                            Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                        });
                    }

                }
            });

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            _footerShow = footerShow;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }
        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, Dictionary<int, string> order = null)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });


            if (order != null)
            {
                List<ColumnsDetail> orderColumn = new List<ColumnsDetail>();

                for (int i = 0; i < columnToShow.Count(); i++)
                {

                    if (order.ContainsKey(i))
                    {
                        string a = order[i];
                        orderColumn.Add(new ColumnsDetail { Name = Convert.ToString(i), Data = order[i] });
                    }
                }
                _order = orderColumn;
            }


            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }

        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, List<string[]> showLink = null)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });

            List<ColumnsDetail> _datashowLink = new List<ColumnsDetail>();
            if (showLink.Count > 0)
            {
                for (int i = 0; i < showLink.Count(); i++)
                {
                    int j = 0;
                    string strType = "";
                    if (showLink[i].Count() < 3)
                        strType = "";
                    else
                        strType = showLink[i][j + 2].ToString();

                    _datashowLink.Add(new ColumnsDetail { Name = showLink[i][j].ToString(), Data = showLink[i][j + 1].ToString(), Type = strType });
                }
                _showLink = _datashowLink;
            }

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }

        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, bool footerShow = false, bool viewShow = true, bool editShow = true)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            _footerShow = footerShow;
            _viewShow = viewShow;
            _editShow = editShow;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }

        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, bool footerShow = false, bool viewShow = true, bool editShow = true, string iconTreeName = "")
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            _footerShow = footerShow;
            _viewShow = viewShow;
            _editShow = editShow;
            _iconTreeName = iconTreeName;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }

        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, bool footerShow = false, bool viewShow = true, bool editShow = true, List<string[]> showLink = null)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });

            List<ColumnsDetail> _datashowLink = new List<ColumnsDetail>();
            if (showLink.Count > 0)
            {
                for (int i = 0; i < showLink.Count(); i++)
                {
                    int j = 0;
                    string strType = "";
                    if (showLink[i].Count() < 3)
                        strType = "";
                    else
                        strType = showLink[i][j + 2].ToString();

                    _datashowLink.Add(new ColumnsDetail { Name = showLink[i][j].ToString(), Data = showLink[i][j + 1].ToString(), Type = strType });
                }
                _showLink = _datashowLink;
            }

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            _footerShow = footerShow;
            _viewShow = viewShow;
            _editShow = editShow;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }

        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, bool footerShow = false, bool viewShow = true, bool editShow = true, Dictionary<int, string> order = null)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });

            if (order != null)
            {
                List<ColumnsDetail> orderColumn = new List<ColumnsDetail>();

                for (int i = 0; i < columnToShow.Count(); i++)
                {

                    if (order.ContainsKey(i))
                    {
                        string a = order[i];
                        orderColumn.Add(new ColumnsDetail { Name = Convert.ToString(i), Data = order[i] });
                    }
                }
                _order = orderColumn;
            }

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            _footerShow = footerShow;
            _viewShow = viewShow;
            _editShow = editShow;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }

        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, bool footerShow = false, bool viewShow = true, bool editShow = true, string iconTreeName = "", List<string[]> showLink = null)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });

            List<ColumnsDetail> _datashowLink = new List<ColumnsDetail>();
            if (showLink.Count > 0)
            {
                for (int i = 0; i < showLink.Count(); i++)
                {
                    int j = 0;
                    string strType = "";
                    if (showLink[i].Count() < 3)
                        strType = "";
                    else
                        strType = showLink[i][j + 2].ToString();

                    _datashowLink.Add(new ColumnsDetail { Name = showLink[i][j].ToString(), Data = showLink[i][j + 1].ToString(), Type = strType });
                }
                _showLink = _datashowLink;
            }

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            _footerShow = footerShow;
            _viewShow = viewShow;
            _editShow = editShow;
            _iconTreeName = iconTreeName;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }

        }
        public DatatableIndexModels(Type entitity, Dictionary<string, string> columnNameToChange, string[] columnToShow, string[] columnHeader, string endFix, string url, string filterString, bool footerShow = false, bool viewShow = true, bool editShow = true, string iconTreeName = "", List<string[]> showLink = null, Dictionary<int, string> order = null)
        {
            List<ColumnsDetail> listOfColumn = new List<ColumnsDetail>();
            var propertyList = entitity.GetProperties().ToList();
            columnToShow.ToList().ForEach(z =>
            {
                PropertyInfo pInfo;
                pInfo = propertyList.Where(x => x.Name == z).FirstOrDefault();
                if (pInfo != null)
                {
                    string name = (columnNameToChange.ContainsKey(pInfo.Name)) ? columnNameToChange[pInfo.Name] : pInfo.Name;
                    listOfColumn.Add(new ColumnsDetail
                    {
                        Name = name,
                        Data = pInfo.Name,
                        Type = (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? pInfo.PropertyType.GetGenericArguments()[0].Name : pInfo.PropertyType.Name
                    });
                }
            });

            List<ColumnsDetail> _datashowLink = new List<ColumnsDetail>();
            if (showLink.Count > 0)
            {
                for (int i = 0; i < showLink.Count(); i++)
                {
                    int j = 0;
                    string strType = "";
                    if (showLink[i].Count() < 3)
                        strType = "";
                    else
                        strType = showLink[i][j + 2].ToString();

                    _datashowLink.Add(new ColumnsDetail { Name = showLink[i][j].ToString(), Data = showLink[i][j + 1].ToString(), Type = strType });
                }
                _showLink = _datashowLink;
            }

            if (order != null)
            {
                List<ColumnsDetail> orderColumn = new List<ColumnsDetail>();

                for (int i = 0; i < columnToShow.Count(); i++)
                {

                    if (order.ContainsKey(i))
                    {
                        string a = order[i];
                        orderColumn.Add(new ColumnsDetail { Name = Convert.ToString(i), Data = order[i] });
                    }
                }
                _order = orderColumn;
            }

            _listOfColumns = listOfColumn;
            _tableId = "TABLE_" + endFix;
            _filterClearButtonId = "FILTER_CLEAR_" + endFix;
            _filterButtonId = "FILTER_BUTTON" + endFix;
            _url = url;
            _filterString = filterString;
            _footerShow = footerShow;
            _viewShow = viewShow;
            _editShow = editShow;
            _iconTreeName = iconTreeName;
            if (columnHeader != null)
            {
                _columnHeader = columnHeader.ToList();
            }

        }

    }

    public class ColumnsDetail
    {
        private string _name;
        private string _type;
        private string _data;

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public string Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        public string Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }
        }

    }
    public partial class PagedResult<T>
    {
        public PagedResult(int pageNumber, int itemsPerPage)
        {
            this.PageNumber = pageNumber;
            this.ItemsPerPage = itemsPerPage;
        }
        public int TotalItems { get; set; }
        public IEnumerable<T> Items { get; set; }
        public int ItemsPerPage { get; set; }
        public int PageNumber { get; set; }

    }

}