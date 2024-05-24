using System.Collections.Immutable;
using System.Text;
using Microsoft.FSharp.Core;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using PomodoroWindowsTimer;
using PomodoroWindowsTimer.Abstractions;
using PomodoroWindowsTimer.Types;

namespace PomodoroWindowsTimer.Exporter.Excel;

public sealed class ExcelBook : IExcelBook
{
    public FSharpResult<IExcelSheet, string> Create(string filePath)
    {
        try
        {
            return FSharpResult<IExcelSheet, string>.NewOk(new ExcelSheet(filePath));
        }
        catch (Exception ex)
        {
            return FSharpResult<IExcelSheet, string>.NewError(ex.Message);
        }
    }

    public FSharpResult<Unit, string> Save(IExcelSheet excelSheet)
    {
        if (excelSheet is ExcelSheet sheet) {
            try
            {
                using (FileStream fs = new FileStream(sheet.FilePath, FileMode.Create))
                {
                    sheet.Workbook.Write(fs, false);
                }

                return FSharpResult<Unit, string>.NewOk(Helpers.Unit.Value);
            }
            catch (Exception ex)
            {
                return FSharpResult<Unit, string>.NewError(ex.Message);
            }
        }

        return FSharpResult<Unit, string>.NewError("Unprocessable excelSheet instance type.");
    }

    private sealed class ExcelSheet : IExcelSheet
    {
        private static string title = "Отчет об оказанных услугах";
        private static string[] headers = [ "№ п/п", "Дата", "Код задачи", "Содержание услуги (название задачи)", "Срок выполнения, часы", "Время завершения"];

        private readonly IWorkbook _workbook; 
        private readonly ISheet _sheet;

        public IWorkbook Workbook => _workbook;

        public string FilePath { get; }

        private Dictionary<string, ICellStyle>? _styles;

        private static Dictionary<string, ICellStyle> GetStyles(ExcelSheet excelSheet)
            => excelSheet._styles ??= createStyles(excelSheet._workbook);

        internal ExcelSheet(string filePath)
        {
            _workbook = new XSSFWorkbook();
            _sheet = _workbook.CreateSheet();
            FilePath = filePath;
        }

        public FSharpResult<int, string> AddHeaders()
        {
            try
            {
                var styles = GetStyles(this);

                IRow headerRow = _sheet.CreateRow(0);

                ICell cell = headerRow.CreateCell(0, CellType.String);
                headerRow.HeightInPoints = (37.5f);
                cell.SetCellValue(title);
                cell.CellStyle = (styles["header"]);

                headerRow = _sheet.CreateRow(1);
                for (int i = 0; i < headers.Length; i++)
                {
                    cell = headerRow.CreateCell(i, CellType.String);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = (styles["cell_h"]);
                }

                CellRangeAddress cellRangeAddress = new CellRangeAddress(0, 0, 0, headers.Length - 1);
                _sheet.AddMergedRegion(cellRangeAddress);

                return FSharpResult<int, string>.NewOk(2);
            }
            catch (Exception ex) {
                return FSharpResult<int, string>.NewError(ex.Message);
            }
        }

        public FSharpResult<int, string> AddRows(DateOnly date, TimeOnly startTime, IEnumerable<ExcelRow> excelRowsEnum, int startRow)
        {
            var excelRows = excelRowsEnum.ToImmutableList();
            var styles = GetStyles(this);

            try
            {
                IRow row = _sheet.CreateRow(startRow);
                ICell cell = row.CreateCell(0);
                cell.CellStyle = (styles["cell_filled"]);
                cell = row.CreateCell(1);
                cell.CellStyle = (styles["cell_filled"]);
                cell = row.CreateCell(2);
                cell.CellStyle = (styles["cell_filled"]);
                cell = row.CreateCell(3);
                cell.CellStyle = (styles["cell_filled"]);
                cell = row.CreateCell(4);
                cell.CellStyle = (styles["cell_filled"]);
                CellRangeAddress cellRangeAddress = new CellRangeAddress(startRow, startRow, 0, 4);
                _sheet.AddMergedRegion(cellRangeAddress);

                cell = row.CreateCell(5);
                cell.SetCellValue($"{startTime.Hour:#0}:{startTime.Minute:00}");
                cell.CellStyle = (styles["cell_normal_time_t"]);

                var prevTime = startTime;
                int i = 0;

                int rowNum;
                Queue<int> workRowNumQueue = new ();

                for (; i < excelRows.Count; i++)
                {
                    rowNum = startRow + 1 + i;
                    row = _sheet.CreateRow(rowNum);

                    switch (excelRows[i].Tag)
                    {
                        case ExcelRow.Tags.WorkExcelRow:
                            prevTime = AddWorkRow(row, date, prevTime, excelRows[i]);
                            workRowNumQueue.Enqueue(rowNum);
                            break;

                        case ExcelRow.Tags.IdleExcelRow:
                            prevTime = AddIdleRow(row, date, prevTime, excelRows[i]);
                            break;
                    }
                }

                rowNum = startRow + 1 + i;
                row = _sheet.CreateRow(rowNum);

                if (workRowNumQueue.Count > 0)
                {
                    AddSummaryCell(row, workRowNumQueue);
                }

                //set column widths, the width is measured in units of 1/256th of a character width
                _sheet.SetColumnWidth(0, 256 * 6);
                _sheet.SetColumnWidth(1, 256 * 17);
                _sheet.SetColumnWidth(2, 256 * 20);
                _sheet.SetColumnWidth(3, 256 * 80);
                _sheet.SetColumnWidth(4, 256 * 17);
                _sheet.SetColumnWidth(5, 256 * 17);

                return FSharpResult<int, string>.NewOk(startRow + 2 + i);
            }
            catch (Exception ex)
            {
                return FSharpResult<int, string>.NewError(ex.Message);
            }
        }

        private void AddSummaryCell(IRow row, Queue<int> workRowNumQueue)
        {
            var styles = GetStyles(this);
            StringBuilder formula = new();

            while (workRowNumQueue.TryDequeue(out int rowNum))
            {
                formula.AppendFormat("E").Append(rowNum + 1).Append("+");
            }

            formula.Remove(formula.Length - 1, 1);
            var cell = row.CreateCell(4);
            cell.SetCellType(CellType.Formula);
            cell.SetCellFormula(formula.ToString());
            cell.CellStyle = (styles["cell_normal_time_h"]);
        }

        private TimeOnly AddWorkRow(IRow row, DateOnly date, TimeOnly prevTime, ExcelRow excelRow)
        {
            ICell cell;
            var styles = GetStyles(this);

            var workRow = (ExcelRow.WorkExcelRow)excelRow;

            cell = row.CreateCell(0);
            cell.SetCellValue(workRow.Item.Num);
            cell.CellStyle = (styles["cell_normal_right"]);

            cell = row.CreateCell(1);
            cell.SetCellValue(date);
            cell.CellStyle = (styles["cell_normal_date"]);

            cell = row.CreateCell(2);
            cell.SetCellValue(workRow.Item.Work.Number);
            cell.CellStyle = (styles["cell_normal_left"]);

            cell = row.CreateCell(3);
            cell.SetCellValue(workRow.Item.Work.Title);
            cell.CellStyle = (styles["cell_normal_left"]);

            cell = row.CreateCell(4);
            //var workDur = workRow.Item.End - prevTime;
            //cell.SetCellValue(new DateTime(date, TimeOnly.FromTimeSpan(workDur)));
            cell.SetCellType(CellType.Formula);
            cell.SetCellFormula($"F{row.RowNum + 1}-F{row.RowNum}");
            cell.CellStyle = (styles["cell_normal_time"]);

            cell = row.CreateCell(5);
            cell.SetCellValue($"{workRow.Item.End.Hour:#0}:{workRow.Item.End.Minute:00}");
            cell.CellStyle = (styles["cell_normal_time"]);

            return workRow.Item.End;
        }

        private TimeOnly AddIdleRow(IRow row, DateOnly date, TimeOnly prevTime, ExcelRow excelRow)
        {
            ICell cell;
            var styles = GetStyles(this);

            var idleRow = (ExcelRow.IdleExcelRow)excelRow;

            cell = row.CreateCell(0);
            cell.SetCellValue(idleRow.Item.Num);
            cell.CellStyle = (styles["cell_normal_right"]);

            cell = row.CreateCell(1);
            cell.SetCellValue(date);
            cell.CellStyle = (styles["cell_normal_date"]);

            cell = row.CreateCell(2);
            cell.SetCellValue("");
            cell.CellStyle = (styles["cell_normal_left_i"]);

            cell = row.CreateCell(3);
            cell.SetCellValue("idle");
            cell.CellStyle = (styles["cell_normal_left_i"]);

            cell = row.CreateCell(4);
            // var idleDur = idleRow.Item.End - prevTime;
            // cell.SetCellValue(new DateTime(date, TimeOnly.FromTimeSpan(idleDur)));
            cell.SetCellType(CellType.Formula);
            cell.SetCellFormula($"F{row.RowNum + 1}-F{row.RowNum}");
            cell.CellStyle = (styles["cell_normal_time_i"]);

            cell = row.CreateCell(5);
            cell.SetCellValue($"{idleRow.Item.End.Hour:#0}:{idleRow.Item.End.Minute:00}");
            cell.CellStyle = (styles["cell_normal_time_i"]);

            return idleRow.Item.End;
        }

        /**
     * create a library of cell styles
     */
        private static Dictionary<String, ICellStyle> createStyles(IWorkbook wb)
        {
            Dictionary<String, ICellStyle> styles = new Dictionary<String, ICellStyle>();
            IDataFormat df = wb.CreateDataFormat();

            ICellStyle style;
            IFont headerFont = wb.CreateFont();
            headerFont.FontHeightInPoints = ((short)14);
            headerFont.IsBold = true;
            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            style.FillForegroundColor = (IndexedColors.LightCornflowerBlue.Index);
            style.FillPattern = FillPattern.SolidForeground;
            style.SetFont(headerFont);
            styles.Add("header", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.FillForegroundColor = (IndexedColors.LightCornflowerBlue.Index);
            style.FillPattern = FillPattern.SolidForeground;
            style.SetFont(headerFont);
            style.DataFormat = (df.GetFormat("d-mmm"));
            styles.Add("header_date", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            style.FillForegroundColor = (IndexedColors.Grey25Percent.Index);
            style.FillPattern = FillPattern.SolidForeground;
            style.SetFont(headerFont);
            styles.Add("cell_filled", style);

            IFont font1 = wb.CreateFont();
            font1.IsBold = true;
            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.SetFont(font1);
            styles.Add("cell_b", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.SetFont(font1);
            styles.Add("cell_b_centered", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.SetFont(font1);
            style.DataFormat = (df.GetFormat("d-mmm"));
            styles.Add("cell_b_date", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.SetFont(font1);
            style.FillForegroundColor = (IndexedColors.Grey25Percent.Index);
            style.FillPattern = FillPattern.SolidForeground;
            style.DataFormat = (df.GetFormat("d-mmm"));
            styles.Add("cell_g", style);

            IFont font2 = wb.CreateFont();
            font2.Color = (IndexedColors.Blue.Index);
            font2.IsBold = true;
            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.SetFont(font2);
            styles.Add("cell_bb", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.SetFont(font1);
            style.FillForegroundColor = (IndexedColors.Grey25Percent.Index);
            style.FillPattern = FillPattern.SolidForeground;
            style.DataFormat = (df.GetFormat("d-mmm"));
            styles.Add("cell_bg", style);

            IFont font3 = wb.CreateFont();
            font3.FontHeightInPoints = ((short)12);
            font3.Color = (IndexedColors.DarkBlue.Index);
            font3.IsBold = true;
            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.SetFont(font3);
            style.WrapText = (true);
            styles.Add("cell_h", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Left;
            style.WrapText = (true);
            styles.Add("cell_normal_left", style);

            IFont font4 = wb.CreateFont();
            font4.FontHeightInPoints = ((short)11);
            font4.Color = (IndexedColors.Grey40Percent.Index);
            font4.IsBold = false;

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Left;
            style.WrapText = (true);
            style.SetFont(font4);
            styles.Add("cell_normal_left_i", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Right;
            style.WrapText = (true);
            styles.Add("cell_normal_right", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.WrapText = (true);
            styles.Add("cell_normal_centered", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Right;
            style.WrapText = (true);
            style.DataFormat = (df.GetFormat("m/d/yyyy"));
            styles.Add("cell_normal_date", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Right;
            style.WrapText = (true);
            style.DataFormat = (df.GetFormat("h:mm"));
            styles.Add("cell_normal_time", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Right;
            style.WrapText = (true);
            style.DataFormat = (df.GetFormat("h:mm"));
            style.SetFont(font4);
            styles.Add("cell_normal_time_i", style);

            IFont fontBold11 = wb.CreateFont();
            fontBold11.FontHeightInPoints = ((short)11);
            fontBold11.IsBold = true;

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Right;
            style.WrapText = (true);
            style.DataFormat = (df.GetFormat("h:mm"));
            style.SetFont(fontBold11);
            style.FillForegroundColor = (IndexedColors.LightYellow.Index);
            style.FillPattern = FillPattern.SolidForeground;
            styles.Add("cell_normal_time_h", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Right;
            style.WrapText = (true);
            style.DataFormat = (df.GetFormat("h:mm"));
            style.SetFont(fontBold11);
            styles.Add("cell_normal_time_t", style);

            style = CreateBorderedStyle(wb);
            style.Alignment = HorizontalAlignment.Center;
            style.Indention = ((short)1);
            style.WrapText = (true);
            styles.Add("cell_indented", style);

            style = CreateBorderedStyle(wb);
            style.FillForegroundColor = (IndexedColors.Blue.Index);
            style.FillPattern = FillPattern.SolidForeground;
            styles.Add("cell_blue", style);

            return styles;
        }

        private static ICellStyle CreateBorderedStyle(IWorkbook wb)
        {
            ICellStyle style = wb.CreateCellStyle();
            style.BorderRight = BorderStyle.Thin;
            style.RightBorderColor = (IndexedColors.Black.Index);
            style.BorderBottom = BorderStyle.Thin;
            style.BottomBorderColor = (IndexedColors.Black.Index);
            style.BorderLeft = BorderStyle.Thin;
            style.LeftBorderColor = (IndexedColors.Black.Index);
            style.BorderTop = BorderStyle.Thin;
            style.TopBorderColor = (IndexedColors.Black.Index);
            return style;
        }
    }
}
