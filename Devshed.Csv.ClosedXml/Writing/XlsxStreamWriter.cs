﻿using ClosedXML.Excel;
using Devshed.Csv.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Devshed.Csv.ClosedXml
{
    /// <summary>
    /// Writes an object model to a XLSX file.
    /// </summary>
    public class XlsxStreamWriter : ICsvStreamWriter
    {
        private readonly string sheetName;

        private readonly SaveOptions options;

        private readonly IStringFormatter formatter;

        /// <summary>
        /// Inititize the writer.
        /// </summary>
        /// <param name="sheetName"> The sheet name. </param>
        /// <param name="options"> Saving options. </param>
        public XlsxStreamWriter(string sheetName = "Document", SaveOptions options = null)
        {
            this.sheetName = sheetName;
            this.options = options;
            this.formatter = new XlsxStringFormatter();
        }

        /// <summary>
        /// Write the object model / collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="rows"></param>
        /// <param name="definition"></param>
        public void Write<T>(Stream stream, T[] rows, CsvDefinition<T> definition)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);

                var rowid = 1;
                if (definition.FirstRowContainsHeaders)
                {
                    this.AddHeader<T>(worksheet, definition, rows, rowid);
                    rowid++;
                }

                foreach (var row in rows)
                {
                    this.AddLine<T>(worksheet, definition, row, rowid);
                    rowid++;
                }

                if (options != null)
                {
                    workbook.SaveAs(stream, options);
                }
                else
                {
                    workbook.SaveAs(stream);
                }
            }
        }

        private void AddLine<T>(IXLWorksheet worksheet, CsvDefinition<T> definition, T item, int rowid)
        {
            var colid = 1;
            foreach (var column in definition.Columns)
            {
                foreach (var value in column.Render(definition, item, definition.FormattingCulture, formatter))
                {
                    var cell = worksheet.Row(rowid).Cell(colid);
                    cell.DataType = GetDataType(column);
                    cell.Value = value;
                    cell.Style.Alignment.WrapText = false;

                    //if (cell.DataType == XLDataType.Text && !value.StartsWith("'"))
                    //{
                    //    cell.Value = "'" + value;
                    //}
                    //else
                    //{
                    //    cell.Value = value;
                    //}

                    colid++;
                }
            }
        }

        private static XLDataType GetDataType<T>(ICsvColumn<T> column)
        {
            switch (column.DataType)
            {
                case ColumnDataType.Number:
                case ColumnDataType.Decimal:
                    return XLDataType.Number;

                case ColumnDataType.DateTime:
                    return XLDataType.DateTime;

                case ColumnDataType.Boolean:
                    return XLDataType.Boolean;

                case ColumnDataType.Time:
                    //return XLDataType.Text;
                    return XLDataType.TimeSpan;

                case ColumnDataType.Currency:
                    return XLDataType.Number;

                case ColumnDataType.Text:
                case ColumnDataType.Composite:
                case ColumnDataType.StrongTyped:
                case ColumnDataType.Object:
                case ColumnDataType.Dynamic:
                    return XLDataType.Text;

                default:
                    return XLDataType.Text;
            }
        }

        private void AddHeader<T>(IXLWorksheet worksheet, CsvDefinition<T> definition, T[] rows, int rowid)
        {
            var headers = definition.Columns.SelectMany(column => GetHeaderNames<T>(definition, column, rows)).ToArray();
            var row = worksheet.Row(rowid);
            var colid = 1;

            foreach (var header in headers)
            {
                row.Cell(colid).Value = header;
                colid++;
            }
        }

        private static HeaderCollection GetHeaderNames<T>(CsvDefinition<T> definition, ICsvColumn<T> column, T[] rows)
        {
            return column.GetWritingHeaderNames(rows);
        }
    }
}
