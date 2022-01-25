using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FastMember;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Security.Claims;
using System.Web;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using static Web.MainApplication.WebUtility.Dictionary;

namespace Web.MainApplication.WebUtility
{
    public static class Function
    {
        private static readonly IDictionary<Type, ICollection<PropertyInfo>> _Properties = new Dictionary<Type, ICollection<PropertyInfo>>();

        public static IQueryable<T> Active<T>(this IQueryable<T> source)
        {
            return source.Where(ActiveString());
        }

        public static IQueryable<T> Active<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            return source.Where(ActiveString()).Where(predicate);
        }

        public static IEnumerable<T> Active<T>(this ICollection<T> source)
        {
            return source.Where(ActiveString());
        }

        public static IEnumerable<T> Active<T>(this ICollection<T> source, Func<T, bool> predicate)
        {
            return source.Where(ActiveString()).Where(predicate);
        }

        public static string ActiveString()
        {
            return "Deleted != true";
        }

        public static DataTable ConvertListToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static DataTable ConvertListToDataTable(this Controller controller, IEnumerable<object> data, List<string> selectList)
        {
            DataTable dt = new DataTable();
            Type myListElementType = data.GetType().GetGenericArguments().Single();
            using (var reader = new ObjectReader(myListElementType, data, selectList.ToArray()))
            {
                dt.Load(reader);
            }

            return dt;


        }

        public static string ConvertToRupiah(this decimal angka)
        {
            return String.Format(CultureInfo.CreateSpecificCulture("id-id"), " {0:N}", angka);
        }

        public static IEnumerable<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                var objType = typeof(T);
                ICollection<PropertyInfo> properties;

                lock (_Properties)
                {
                    if (!_Properties.TryGetValue(objType, out properties))
                    {
                        properties = objType.GetProperties().Where(property => property.CanWrite).ToList();
                        _Properties.Add(objType, properties);
                    }
                }

                var list = new List<T>(table.Rows.Count);

                //foreach (var row in table.AsEnumerable().Skip(1))  /if there are title of column
                foreach (var row in table.AsEnumerable())
                {
                    var obj = new T();

                    foreach (var prop in properties)
                    {
                        try
                        {
                            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            var safeValue = row[prop.Name] == null ? null : Convert.ChangeType(row[prop.Name], propType);

                            prop.SetValue(obj, safeValue, null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return Enumerable.Empty<T>();
            }
        }

        public static Dictionary<string, string> GenerateDisplayname<T>(T model)
        {
            PropertyInfo[] listPI = model.GetType().GetProperties();
            Dictionary<string, string> dictDisplayNames = new Dictionary<string, string>();
            string displayName = string.Empty;

            foreach (PropertyInfo pi in listPI)
            {
                DisplayNameAttribute dp = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
                if (dp != null)
                {
                    displayName = dp.DisplayName;
                    dictDisplayNames.Add(pi.Name, displayName);
                }
            }
            return dictDisplayNames;
        }

        public static byte[] GenerateExcel(this Controller controller, List<string> selectList, List<string> columnHeader, IEnumerable<object> data, string worksheetName, List<SFormat> SFormat = null, WorkSheetFormat workSheetFormat = null)
        {
            if (SFormat != null && SFormat.Any())
            {

                if (worksheetName.Length > ExportFileNameTitle.MaxSheetName)
                {
                    worksheetName = worksheetName.Substring(0, ExportFileNameTitle.MaxSheetName);
                }
                var table = new System.Data.DataTable();
                Type myListElementType = data.GetType().GetGenericArguments().Single();
                var newReader = new ObjectReader(myListElementType, data, selectList.ToArray());
                using (var reader = new ObjectReader(myListElementType, data, selectList.ToArray()))
                {
                    table.Load(reader);
                }
                //using (var reader = objectreader.create(data, selectlist.toarray()))
                //{
                //    table.load(reader);
                //}

                //using (var reader = ObjectReader.Create(data))
                //{
                //    table.Load(reader);
                //}
                using (var workbook = new XLWorkbook())
                {
                    int rowCounter = 1;
                    var worksheet = workbook.Worksheets.Add(worksheetName);
                    worksheet.Columns().AdjustToContents();

                    int colCounter = 0;
                    foreach (var item in columnHeader)
                    {
                        colCounter++;
                        worksheet.Cell(rowCounter, colCounter).Value = item;
                    }

                    rowCounter++;


                    foreach (DataRow item in table.Rows)
                    {
                        for (int i = 0; i < selectList.Count; i++)
                        {
                            var dataCell = item[selectList.ElementAtOrDefault(i)];
                            if (dataCell is DateTime)
                            {
                                worksheet.Cell(rowCounter, i + 1).Value = dataCell;

                                worksheet.Cell(rowCounter, i + 1).Style.NumberFormat.Format = "dd MMMM yyyy";
                            }
                            else if (dataCell is string)
                            {
                                worksheet.Cell(rowCounter, i + 1).SetValue(dataCell);
                            }
                            else
                            {
                                worksheet.Cell(rowCounter, i + 1).Value = dataCell;
                            }
                            var sf = SFormat.Where(x => x.PropertyName == selectList.ElementAtOrDefault(i)).FirstOrDefault();
                            if (sf != null)
                            {
                                worksheet.Cell(rowCounter, i + 1).Value = string.Format(sf.StringFormat, dataCell);
                            }
                        }
                        rowCounter++;
                    }
                    if (workSheetFormat != null && workSheetFormat.BorderCell)
                    {
                        if (workSheetFormat.BorderCell)
                        {
                            worksheet.Range(1, 1, data.Count() + 1, columnHeader.Count).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            worksheet.Range(1, 1, data.Count() + 1, columnHeader.Count).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            worksheet.Range(1, 1, data.Count() + 1, columnHeader.Count).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            worksheet.Range(1, 1, data.Count() + 1, columnHeader.Count).Style.Border.LeftBorder = XLBorderStyleValues.Thin;

                        }
                        if (workSheetFormat.BorderCell)
                        {
                            worksheet.Range(1, 1, 1, columnHeader.Count).Style.Font.SetBold();
                        }

                    }

                    using (MemoryStream stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }


                }
            }
            else
            {

                if (worksheetName.Length > ExportFileNameTitle.MaxSheetName)
                {
                    worksheetName = worksheetName.Substring(0, ExportFileNameTitle.MaxSheetName);
                }
                var table = new System.Data.DataTable();
                Type myListElementType = data.GetType().GetGenericArguments().Single();
                var newReader = new ObjectReader(myListElementType, data, selectList.ToArray());
                using (var reader = new ObjectReader(myListElementType, data, selectList.ToArray()))
                {
                    table.Load(reader);
                }
                //using (var reader = ObjectReader.Create(data, selectList.ToArray()))
                //{
                //    table.Load(reader);
                //}
                using (var workbook = new XLWorkbook())
                {
                    int rowCounter = 1;
                    var worksheet = workbook.Worksheets.Add(worksheetName);
                    worksheet.Columns("E").AdjustToContents();

                    int colCounter = 0;
                    foreach (var item in columnHeader)
                    {
                        colCounter++;
                        worksheet.Cell(rowCounter, colCounter).Value = item;
                    }

                    rowCounter++;


                    foreach (DataRow item in table.Rows)
                    {
                        for (int i = 0; i < selectList.Count; i++)
                        {
                            var dataCell = item[selectList.ElementAtOrDefault(i)];
                            if (dataCell is DateTime)
                            {
                                worksheet.Cell(rowCounter, i + 1).Value = dataCell;



                                worksheet.Cell(rowCounter, i + 1).Style.NumberFormat.Format = "dd MMMM yyyy";
                            }
                            else if (dataCell is string)
                            {
                                //cek jika dataCell mengandung ";"
                                var dataTmp = dataCell.ToString();
                                var dataCek = dataTmp.Split(';');
                                if (dataCek.Length > 2)
                                {

                                    //var dataNonXML = dataCell.ToString();
                                    var cetak = Regex.Replace(dataTmp, "<.*?>", String.Empty);
                                    cetak.ToString();
                                    worksheet.Cell(rowCounter, i + 1).SetValue(cetak.Substring(0, cetak.Length - 1));

                                    //worksheet.Cell(rowCounter, i + 1).SetValue(dataNonXML);
                                }
                                else
                                {
                                    worksheet.Cell(rowCounter, i + 1).SetValue(Regex.Replace(dataCek[0], "<.*?>", String.Empty));
                                }
                                //worksheet.Cell(rowCounter, i + 1).SetValue(dataCell);
                            }
                            else
                            {
                                worksheet.Cell(rowCounter, i + 1).Value = dataCell;
                            }
                        }
                        rowCounter++;
                    }

                    if (workSheetFormat != null && workSheetFormat.BorderCell)
                    {
                        if (workSheetFormat.BorderCell)
                        {
                            worksheet.Range(1, 1, data.Count() + 1, columnHeader.Count).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            worksheet.Range(1, 1, data.Count() + 1, columnHeader.Count).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            worksheet.Range(1, 1, data.Count() + 1, columnHeader.Count).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            worksheet.Range(1, 1, data.Count() + 1, columnHeader.Count).Style.Border.LeftBorder = XLBorderStyleValues.Thin;

                        }
                        if (workSheetFormat.BorderCell)
                        {
                            worksheet.Range(1, 1, 1, columnHeader.Count).Style.Font.SetBold();
                        }

                    }

                    using (MemoryStream stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }

        }

        public static byte[] GenerateExcel(this Controller controller, List<string> selectList, List<string> columnHeader, IEnumerable<Sheet> sheet, List<SFormat> SFormat = null)
        {
            try
            {

                if (SFormat != null && SFormat.Any())
                {
                    //using (var reader = ObjectReader.Create(data, selectList.ToArray()))
                    //{
                    //    table.Load(reader);
                    //}
                    using (var workbook = new XLWorkbook())
                    {
                        foreach (var item in sheet)
                        {
                            var table = new System.Data.DataTable();
                            Type myListElementType = item.SheetData.GetType().GetGenericArguments().Single();
                            //var newReader = new ObjectReader(myListElementType, data, selectList.ToArray());
                            using (var reader = new ObjectReader(myListElementType, item.SheetData, selectList.ToArray()))
                            {
                                table.Load(reader);
                            }

                            if (item.SheetName.Length > ExportFileNameTitle.MaxSheetName)
                            {
                                item.SheetName = item.SheetName.Substring(0, ExportFileNameTitle.MaxSheetName);
                                //workbook.Worksheets.Add(item.SheetName);
                                int rowCounter = 1;
                                var worksheet = workbook.Worksheets.Add(item.SheetName);
                                int colCounter = 0;
                                foreach (var itemCH in columnHeader)
                                {
                                    colCounter++;
                                    worksheet.Cell(rowCounter, colCounter).Value = itemCH;
                                }

                                rowCounter++;


                                foreach (DataRow itemDR in table.Rows)
                                {
                                    for (int i = 0; i < selectList.Count; i++)
                                    {
                                        var dataCell = itemDR[selectList.ElementAtOrDefault(i)];
                                        if (dataCell is DateTime)
                                        {
                                            worksheet.Cell(rowCounter, i + 1).Value = dataCell;
                                            worksheet.Cell(rowCounter, i + 1).Style.NumberFormat.Format = "dd MMMM yyyy";
                                        }
                                        else if (dataCell is string)
                                        {
                                            worksheet.Cell(rowCounter, i + 1).SetValue(dataCell);
                                        }
                                        else
                                        {
                                            worksheet.Cell(rowCounter, i + 1).Value = dataCell;
                                        }
                                        var sf = SFormat.Where(x => x.PropertyName == selectList.ElementAtOrDefault(i)).FirstOrDefault();
                                        if (sf != null)
                                        {
                                            worksheet.Cell(rowCounter, i + 1).Value = string.Format(sf.StringFormat, dataCell);
                                        }
                                    }
                                    rowCounter++;
                                }


                            }
                        }
                        using (MemoryStream stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            return stream.ToArray();
                        }
                    }
                }
                else
                {

                    //using (var reader = ObjectReader.Create(data, selectList.ToArray()))
                    //{
                    //    table.Load(reader);
                    //}
                    using (var workbook = new XLWorkbook())
                    {
                        foreach (var item in sheet)
                        {
                            var table = new System.Data.DataTable();
                            Type myListElementType = item.SheetData.GetType().GetGenericArguments().Single();
                            //var newReader = new ObjectReader(myListElementType, data, selectList.ToArray());
                            using (var reader = new ObjectReader(myListElementType, item.SheetData, selectList.ToArray()))
                            {
                                table.Load(reader);
                            }

                            if (item.SheetName.Length > ExportFileNameTitle.MaxSheetName)
                            {
                                item.SheetName = item.SheetName.Substring(0, ExportFileNameTitle.MaxSheetName);

                            }
                            //workbook.Worksheets.Add(item.SheetName);
                            int rowCounter = 1;
                            var worksheet = workbook.Worksheets.Add(item.SheetName);
                            int colCounter = 0;
                            foreach (var itemCH in columnHeader)
                            {
                                colCounter++;
                                worksheet.Cell(rowCounter, colCounter).Value = itemCH;
                            }

                            rowCounter++;


                            foreach (DataRow itemDR in table.Rows)
                            {
                                for (int i = 0; i < selectList.Count; i++)
                                {
                                    var dataCell = itemDR[selectList.ElementAtOrDefault(i)];
                                    if (dataCell is DateTime)
                                    {
                                        worksheet.Cell(rowCounter, i + 1).Value = dataCell;
                                        worksheet.Cell(rowCounter, i + 1).Style.NumberFormat.Format = "dd MMMM yyyy";
                                    }
                                    else if (dataCell is string)
                                    {
                                        worksheet.Cell(rowCounter, i + 1).SetValue(dataCell);
                                    }
                                    else
                                    {
                                        worksheet.Cell(rowCounter, i + 1).Value = dataCell;
                                    }

                                }
                                rowCounter++;
                            }


                        }
                        using (MemoryStream stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            return stream.ToArray();
                        }
                    }
                }
            }
            catch (Exception e)
            {

                throw;
            }

        }

        public static string GetIPAddress(this HttpContext httpContext)
        {
            //System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return httpContext.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static bool IsAlphanumeric(this string text)
        {
            return text != null ? text.All(x => char.IsLetterOrDigit(x)) : false;
        }

        public static bool IsAlphanumericOrWhitespace(this string text)
        {
            return text != null ? text.All(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x)) : false;
        }

        public static bool IsLetterOrWhiteSpace(this string text)
        {
            return text != null ? text.All(x => char.IsLetter(x) || char.IsWhiteSpace(x)) : false;
        }

        public static string JsonSerialize(this object obj)
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new MyContractResolver();
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(obj, settings);
        }
        //public static List<string> MessageToList(this Exception e)
        //{
        //    var retval = new List<string>();
        //    if (e.Message != null)
        //    {
        //        retval.Add(e.Message);
        //    }
        //    if (e.GetType().Name == "DbEntityValidationException")
        //    {
        //        var ex = (DbEntityValidationException)e;
        //        ex.EntityValidationErrors.ToList().ForEach(x =>
        //        {
        //            x.ValidationErrors.ToList().ForEach(z =>
        //            {
        //                retval.Add(z.ErrorMessage);
        //            });
        //        });
        //    }
        //    if (e.InnerException != null)
        //    {
        //        retval.AddRange(e.InnerException.MessageToList());
        //    }
        //    return retval;

        //}

        public static IList<T> OrEmptyIfNull<T>(this IList<T> source)
        {
            return source ?? Array.Empty<T>();
        }

        public static ICollection<T> OrEmptyIfNull<T>(this ICollection<T> source)
        {
            return source ?? Array.Empty<T>();
        }

        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Array.Empty<T>();
        }

        public static List<T> PopulateData<T, V>(List<V> data, T model) where T : class, new()
        {
            List<T> oList = new List<T>();
            T resource = (T)Activator.CreateInstance(typeof(T));
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            PropertyDescriptorCollection propertiesData = TypeDescriptor.GetProperties(typeof(V));
            Type type = resource.GetType();

            foreach (var dt in data)
            {
                T source = new T();
                foreach (PropertyDescriptor prop in properties)
                {
                    PropertyInfo propertyInfo = type.GetProperty(prop.Name);
                    var obj = propertiesData.Find(prop.Name, true);
                    if (obj != null)
                    {
                        var value = obj.GetValue(dt);
                        propertyInfo.SetValue(source, value);
                    }
                }
                oList.Add(source);
            }

            return (List<T>)oList;
        }

        public static String SHA1(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static int ToAngka(this string rupiah)
        {
            return int.Parse(Regex.Replace(rupiah, @",.*|\D", ""));
        }

        //internal static Dictionary<string, Component> GenerateClassAtribute<T>(T model)
        //{
        //    PropertyInfo[] listPI = model.GetType().GetProperties();
        //    ExDictionary<string, Component> dictSum = new ExDictionary<string, Component>();
        //    string displayName = string.Empty;

        //    foreach (PropertyInfo pi in listPI)
        //    {
        //        DisplayNameAttribute dp = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault();
        //        DescriptionAttribute de = pi.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>().SingleOrDefault();
        //        bool isHaveTotal = false;

        //        if (dp != null)
        //        {
        //            displayName = dp.DisplayName;
        //            if (de != null)
        //            {
        //                isHaveTotal = true;
        //            }
        //            dictSum.Add(pi.Name, new Component { DisplayName = displayName, HaveTotal = isHaveTotal });
        //        }
        //    }
        //    return dictSum;
        //}

        public class MyContractResolver : DefaultContractResolver
        {

            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                var members = base.GetSerializableMembers(objectType);
                var filteredMembers = new List<MemberInfo>();
                members.ForEach(m =>
                {
                    if (m.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo info = (PropertyInfo)m;
                        var type = info.PropertyType;
                        if (type.IsPrimitive)
                        {
                            filteredMembers.Add(m);
                        }
                        else if (type == typeof(string))
                        {
                            filteredMembers.Add(m);
                        }
                        else if (type == typeof(decimal))
                        {
                            filteredMembers.Add(m);
                        }
                        else if (type == typeof(decimal?))
                        {
                            filteredMembers.Add(m);
                        }
                        else if (type == typeof(bool?))
                        {
                            filteredMembers.Add(m);
                        }
                        else if (type == typeof(DateTime))
                        {
                            filteredMembers.Add(m);
                        }
                        else if (type == typeof(DateTime?))
                        {
                            filteredMembers.Add(m);
                        }
                    }
                });
                return filteredMembers;
            }
        }
        /**
         * // Usage example: //
         * int angka = 10000000;
         * System.Console.WriteLine(angka.ToRupiah()); // -> 10.000.000
         */
    }

    public class Dictionary
    {
        public class ExDictionary<TKey, TValue> : Dictionary<TKey, TValue> { }
        public class ExDictionary<TKey1, TKey2, TValue> : Dictionary<TKey1, ExDictionary<TKey2, TValue>> { }
    }
    public class SFormat
    {
        private string propertyName;
        private string stringFormat;

        public string PropertyName
        {
            get { return propertyName; }
            set { propertyName = value; }
        }
        public string StringFormat
        {
            get { return stringFormat; }
            set { stringFormat = value; }
        }
    }
    public class Sheet
    {
        private IEnumerable<object> sheetData;
        private string sheetName;

        public IEnumerable<object> SheetData
        {
            get { return sheetData; }
            set { sheetData = value; }
        }

        public string SheetName
        {
            get { return sheetName; }
            set { sheetName = value; }
        }
    }

    public class WorkSheetFormat
    {
        public bool BoldHeader;
        public bool BorderCell;
    }

    public static class ExportFileNameTitle
    {
        public const int MaxSheetName = 31;
        public static string ActivityLog() { return "ActivityLog_" + FormatTime(); }

        public static string CustomerInformation() { return "CustomerInformation_" + FormatTime(); }

        public static string FormatTime()
        {
            return DateTime.Now.ToString("ddMMMyyyy_HHmm");
        }

        public static string ImplementationLog() { return "ImplementationLog_" + FormatTime(); }
        public static string TemporaryDeleteCustomer() { return "TemporaryDeletedCustomer_" + FormatTime(); }
        public static string TemporaryDeleteImplementationLog() { return "TemporaryDeletedImplementationLog_" + FormatTime(); }
        public static string TemporaryDeleteProductLibrary() { return "TemporaryDeletedProductLibrary_" + FormatTime(); }


    }
}
