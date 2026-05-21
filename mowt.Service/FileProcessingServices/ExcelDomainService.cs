using NPOI.SS.Formula.Functions;
using Npoi.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;

namespace mowt.Service.FileProcessingServices
{
    public class ExcelDomainService : IExcelDomainService
    {
        public async Task<List<Dictionary<string, object>>> ImportExcelRecords(MemoryStream file)
        {
            return await Task.Run(() =>
            {
                var mapper = new Mapper(file);
                string firstSheetName = mapper.Workbook.GetSheetAt(0).SheetName;
                var objects = mapper.Take<dynamic>(firstSheetName).ToList();

                var result = new List<Dictionary<string, object>>();
                foreach (var obj in objects)
                {
                    var dict = new Dictionary<string, object>();
                    var props = obj.Value.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        dict[prop.Name] = prop.GetValue(obj.Value) ?? "null";
                    }
                    result.Add(dict);
                }
                return result;
            });
        }

        public async Task<MemoryStream> ExportExcelRecords<T>(List<T> records, List<string> selectedColumns, string sheetName)
        {
            return await Task.Run(() =>
            {
                var mapper = new Mapper();
                if (selectedColumns != null)
                {
                    var propertiesToIgnore = typeof(T).GetProperties()
                        .Where(p => !selectedColumns.Contains(p.Name))
                        .Select(p => p.Name);
                    foreach (var prop in propertiesToIgnore)
                    {
                        mapper.Ignore<T>(prop);
                    }
                }
                mapper.Put(records, sheetName: sheetName);
                var memoryStream = new MemoryStream();
                mapper.Save(memoryStream, leaveOpen: true);
                memoryStream.Position = 0; // Reset the stream position to the beginning
                return memoryStream;
            });
        }

    }
}
