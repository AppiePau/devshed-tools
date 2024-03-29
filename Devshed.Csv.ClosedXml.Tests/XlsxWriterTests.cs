﻿namespace Devshed.Csv.ClosedXml.Tests
{
    using Devshed.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class XlsxWriterTests
    {
        #region Constants

        private static readonly string UTF8Bom = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        private static TestRow[] oneRow = new[] { new TestRow { Id = 1, Name = "OK_NAME", Getallen = new[] {
             new CompositeColumnValue<decimal>("COL1", 1.45433M) }, IsActive = true } };

        private static TestRow[] twoRows = new[] {
                new TestRow { Id = 1, Name = "OK_NAME1",
                    GetalDecimal = 1234.56790M,
                    GetalDecimalNullable = 2341.56790M,
                    Getallen = new [] {
                        new CompositeColumnValue<decimal>("COL1", 123.23467M),
                        new CompositeColumnValue<decimal>("COL2", 234.34645M) }, IsActive = true },
                new TestRow { Id = 2, Name = "OK_NAME2",
                    GetalDecimal = 1234.56790M,
                    Getallen = new [] {
                        new CompositeColumnValue<decimal>("COL1", 345.454343M),
                        new CompositeColumnValue<decimal>("COL2", 567.344567M) }, IsActive = false } };

        private static TestRow[] incompleteRows = new[] {
                new TestRow { Id = 1, Name = "OK_NAME1",
                    GetalDecimal = 1234.56790M,
                    GetalDecimalNullable = 2341.56790M,
                    Getallen = new [] {
                        new CompositeColumnValue<decimal>("COL1", 123.243323M),
                        new CompositeColumnValue<decimal>("COL2", 234.345445M) }, IsActive = true },
                new TestRow { Id = 2, Name = "OK_NAME2",
                    GetalDecimal = 1234.56790M,
                    Getallen = new [] {
                        new CompositeColumnValue<decimal>("COL1", 345.454543M),
                        new CompositeColumnValue<decimal>("COL2", 567.563457M),
                        new CompositeColumnValue<decimal>("COL3", 678.563457M) }, IsActive = false },
                new TestRow { Id = 2, Name = "OK_NAME2",
                    GetalDecimal = 1234.56790M,
                    Getallen = new [] {
                        new CompositeColumnValue<decimal>("COL2", 567.567901M) }, IsActive = false } };


        #endregion

        [TestMethod]
        public void Build_OneTestRow_CreatesCsv()
        {
            var result = NameDefinition().WriteAsXlsx(oneRow);

            using (var s = new FileStream(".\\Test_" + DateTime.Now.Ticks + ".xlsx", FileMode.CreateNew))
            {
                s.Write(result.GetBytes(), 0, (int)result.Length);
            }

        }


        [TestMethod]
        public void Build_TwoTestRowsWithHeader_CreatesCsv()
        {
            var result = FullDefinitionWithHeaders(twoRows).WriteAsXlsx(twoRows);
            using (var s = new FileStream(".\\Test_" + DateTime.Now.Ticks + ".xlsx", FileMode.CreateNew))
            {
                s.Write(result.GetBytes(), 0, (int)result.Length);
            }
        }

        [TestMethod]
        public void Build_IncompleteComposite_CreatesCsv()
        {
            var result = FullDefinitionWithHeadersAndIncomplete(incompleteRows).WriteAsXlsx(incompleteRows);
            using (var s = new FileStream(".\\Test_" + DateTime.Now.Ticks + ".xlsx", FileMode.CreateNew))
            {
                s.Write(result.GetBytes(), 0, (int)result.Length);
            }
        }

        private static CsvDefinition<TestRow> NameDefinition()
        {
            return new CsvDefinition<TestRow>(
                new TextCsvColumn<TestRow>(e => e.Name)
                {
                    HeaderName = "OK_NAME_HEADER"
                })
            {
                FirstRowContainsHeaders = true,
                WriteBitOrderMarker = false,
                Encoding = Encoding.UTF8
            };
        }

        private static CsvDefinition<TestRow> FullDefinition()
        {
            return new CsvDefinition<TestRow>(
                  new NumberCsvColumn<TestRow>(e => e.Id),
                  new TextCsvColumn<TestRow>(e => e.Name),
                  new BooleanCsvColumn<TestRow>(e => e.IsActive))
            {
                FirstRowContainsHeaders = true,
                WriteBitOrderMarker = false
            };
        }

        private static CsvDefinition<TestRow> FullDefinitionWithHeaders(TestRow[] rows)
        {
            return new CsvDefinition<TestRow>(
                 new NumberCsvColumn<TestRow>(e => e.Id)
                 {
                     HeaderName = "OK_ID_HEADER"
                 },
                 new TextCsvColumn<TestRow>(e => e.Name)
                 {
                     HeaderName = "OK_NAME_HEADER"
                 },
                 new DecimalXlsColumn<TestRow>(e => e.GetalDecimal),
                 new DecimalXlsColumn<TestRow>(e => e.GetalDecimalNullable),
                 new CompositeNumberCsvColumn<TestRow>(e => e.Getallen,
                    rows.SelectMany(e => e.Getallen))
                 {
                     HeaderName = "OK_GETALLEN_HEADER"
                 },
                 new BooleanCsvColumn<TestRow>(e => e.IsActive)
                 {
                     HeaderName = "OK_ISACTIVE_HEADER"
                 })
            {
                FirstRowContainsHeaders = true,
                WriteBitOrderMarker = false
            };
        }

        private static CsvDefinition<TestRow> FullDefinitionWithHeadersAndIncomplete(TestRow[] rows)
        {
            return new CsvDefinition<TestRow>(
                 new NumberCsvColumn<TestRow>(e => e.Id)
                 {
                     HeaderName = "OK_ID_HEADER"
                 },
                 new TextCsvColumn<TestRow>(e => e.Name)
                 {
                     HeaderName = "OK_NAME_HEADER"
                 },
                 new DecimalXlsColumn<TestRow>(e => e.GetalDecimal),
                 new DecimalXlsColumn<TestRow>(e => e.GetalDecimalNullable),
                 new CompositeNumberCsvColumn<TestRow>(e => e.Getallen,
                    rows.SelectMany(e => e.Getallen))
                 {
                     HeaderName = "OK_GETALLEN_HEADER",
                     AllowUndefinedColumnsInCollection = true
                 },
                 new BooleanCsvColumn<TestRow>(e => e.IsActive)
                 {
                     HeaderName = "OK_ISACTIVE_HEADER"
                 })
            {
                FirstRowContainsHeaders = true,
                WriteBitOrderMarker = false
            };
        }

        private sealed class TestRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public decimal GetalDecimal { get; set; }
            public decimal? GetalDecimalNullable { get; set; }
            public CompositeColumnValue<decimal>[] Getallen { get; set; }
        }
    }
}
