using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace DesignLibrary.Components.MDatePicker;

public partial class MDatePicker : ComponentBase
{
    [Inject] IJSRuntime JsRuntime { get; set; }
    
    string InternalValue { get; set; }
    [Parameter] public DateTime? Value { get; set; }
    [Parameter] public EventCallback<DateTime?> ValueChanged { get; set; }
    [Parameter] public string Style { get; set; }
    [Parameter] public byte MaxLength { get; set; } = 100;
    string Format { get; set; } = "mm/dd/yyyy";
    string Placeholder { get; set; } = "mm/dd/yyyy";

    Guid _idInput = Guid.NewGuid();

    string[] _monthNames = DateTimeFormatInfo.InvariantInfo.MonthNames;
    string[] _dayNames = DateTimeFormatInfo.InvariantInfo.AbbreviatedDayNames;
    int _chooseMonth = 1;
    int _chooseYear = 2000;
    List<string> _calendarArray = new();
    
    [JSInvokable("OnChangeInternal")]
    public async void OnChangeInternal(ChangeEventArgs args, bool paste = false)
    {
        string val = $"{args.Value}";
        
        int caretMove = 0;
        string[] splitValue = val.Split("/");
        if (splitValue.Length > 0) // month
        {
            try
            {
                if (splitValue[0].Length == 1)
                {
                    int month = int.Parse(splitValue[0]);
                    if (month >= 2)
                    {
                        splitValue[0] = $"0{splitValue[0]}";
                        caretMove += 2;
                    }
                }
            }
            catch(Exception){}
        }
        if (splitValue.Length > 1 && !string.IsNullOrEmpty(splitValue[1])) // day
        {
            try
            {
                int month = int.Parse(splitValue[0]);
                int dayInMonth = DateTimeFormatInfo.InvariantInfo.Calendar.GetDaysInMonth(2000, month);
                int day = int.Parse(splitValue[1]);
                if (splitValue[1].Length > 2)
                {
                    day = int.Parse(splitValue[1].Substring(0,2));
                }
                if (splitValue[1].Length == 1)
                {
                    if (day >= 4)
                    {
                        splitValue[1] = $"0{splitValue[1]}";
                        caretMove += 2;
                    }
                }
                else
                {
                    if (day > dayInMonth)
                    {
                        splitValue[1] = $"{dayInMonth}";
                        caretMove += 2;
                    }
                }
            }
            catch (Exception) { }
        }
        
        StringBuilder value = new(string.Join("/", splitValue));
        
        InternalValue ??= "";
        if (paste)
        {
            value = new StringBuilder(Regex.Replace($"{value}", "[^0-9]", ""));
        }
        
        int positionCaret = await JsRuntime.InvokeAsync<int>("getCaret", _idInput);
        
        if (Format is not null)
        {
            if (InternalValue.Length > value.Length && !paste)
            {
                foreach (char divider in _dividers)
                {
                    value = value.Replace($"{divider}", "");
                }
                
                value.Append('@');
                for (byte a = 0; a < _countNumber.Count; a++)
                {
                    if (positionCaret == _countNumber[a])
                    {
                        if (positionCaret < value.Length-1)
                        {
                            value.Remove(positionCaret - 1, 1);
                            value = value.Insert(_countNumber[a], $"{_dividers[a]}");
                        }
                        else
                        {
                            if (positionCaret != 0)
                            {
                                byte countDividersRemove = 1;
                                (byte, byte) tupleDividers = _moreThenOneDivider.FirstOrDefault(t => t.Item1 - 1 == positionCaret-1);
                                if (tupleDividers != (0,0))
                                {
                                    countDividersRemove = tupleDividers.Item2;
                                }
                                value.Remove(positionCaret - countDividersRemove, countDividersRemove);
                            }
                        }
                        break;
                    }
                    if (value.Length < _countNumber[a]+1) break;

                    value = value.Insert(_countNumber[a], $"{_dividers[a]}");
                }
                
                value.Remove(value.Length - 1, 1);
            }
            else
            {
                foreach (char divider in _dividers)
                {
                    value = value.Replace($"{divider}", "");
                }

                foreach (char divider in _dividers)
                {
                    InternalValue = InternalValue.Replace($"{divider}", "");
                }

                value.Append('@');
                
                for (byte a = 0; a < _countNumber.Count; a++)
                {
                    if (value.Length < _countNumber[a]+1) break;
                    value = value.Insert(_countNumber[a], $"{_dividers[a]}");
                }
                
                value.Remove(value.Length - 1, 1);
                
            }
        }


        string newValue = value.ToString().Length > MaxLength
            ? value.ToString().Substring(0, MaxLength)
            : value.ToString();

        InternalValue = newValue;
        StateHasChanged();

        byte countDividersAddToCaret = 1;
        (byte, byte) tuple = _moreThenOneDivider.FirstOrDefault(t => t.Item1-t.Item2+1 == positionCaret);
        if (tuple != (0,0))
        {
            countDividersAddToCaret = (byte)(tuple.Item2+1);
        }
        
        if (positionCaret+countDividersAddToCaret >= value.Length && positionCaret != MaxLength-2 || paste)
        {
            await JsRuntime.InvokeVoidAsync("setCaret", _idInput, value.Length);
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("setCaret", _idInput, positionCaret+caretMove);
        }
        
        string[] splitNewValue = newValue.Split("/");
        if (splitNewValue.Length >= 1 && splitNewValue[0].Length == 2)
        {
            _chooseMonth = int.Parse(splitNewValue[0]);
        }
        if (splitNewValue.Length >= 3 && splitNewValue[2].Length == 4)
        {
            OnFocusOut();
        }
    }
    
    List<char> _dividers = new ();
    List<byte> _countNumber = new();
    List<(byte, byte)> _moreThenOneDivider = new();

    protected override void OnInitialized()
    {
        if(Value is not null)
        {
            InternalValue = Value.Value.ToString("MM/dd/yyyy", new CultureInfo("en"));
            _chooseMonth = Value.Value.Month;
            
            _chooseYear = Value.Value.Year;
            
            StateHasChanged();
        }
        
        if (Format is null) return;
        
        MaxLength = (byte)Format.Length;
        for(byte a = 0; a < Format.Length; a++)
        {
            if (char.IsLetterOrDigit(Format[a])) continue;

            byte countDivider = CheckCountDividers(Format, a);
            if (countDivider > 1)
            {
                _moreThenOneDivider.Add((a, countDivider));
            }
            _countNumber.Add(a);
            _dividers.Add(Format[a]);
        }

        List<(byte, byte)> removeList = new();
        
        for (byte a = 1; a < _moreThenOneDivider.Count-1; a++)
        {
             if (_moreThenOneDivider[a].Item1 - _moreThenOneDivider[a - 1].Item1 == 1)
             {
                 removeList.Add(_moreThenOneDivider[a - 1]);
             }
        }

        foreach ((byte, byte) valueTuple in removeList)
        {
            _moreThenOneDivider.Remove(valueTuple);
        }
    }

    byte CheckCountDividers(string value, byte position)
    {
        byte returnValue = 1;
        if (position > 0)
        {
            if (!char.IsLetterOrDigit(value[position]))
            {
                if (!char.IsLetterOrDigit(value[position - 1]))
                {
                    returnValue += CheckCountDividers(value, (byte)(position-1));
                }
            }
        }
        return returnValue;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            DotNetObjectReference<MDatePicker> dotNetHelper = DotNetObjectReference.Create(this);
            JsRuntime.InvokeVoidAsync("addPasteLogic", _idInput,dotNetHelper);
            JsRuntime.InvokeVoidAsync("addBackspaceKeyPress", _idInput, _countNumber);
        }
    }

    void OnFocusOut()
    {
        string[] splitValue = InternalValue.Split("/");
        
        if (splitValue.Length < 3 || splitValue[2].Length < 2) return;
        
        int month = int.Parse(splitValue[0]);
        int day = int.Parse(splitValue[1]);
        int year = int.Parse(splitValue[2]);

        if (month is > 12 or < 1) month = 12;
        
        int daysInMonth = DateTimeFormatInfo.InvariantInfo.Calendar.GetDaysInMonth(year, month);

        if (day > daysInMonth) day = daysInMonth;

        DateTime now = DateTime.Now;
        
        if (splitValue[2].Length == 2)
        {
            if (year <= now.Year - 2000)
            {
                year += 2000;
            }
            else
            {
                year += 1900;
            }
        }

        DateTime dateTime = new (year, month, day);
        _chooseMonth = month;
        _chooseYear = year;
        InternalValue = dateTime.ToString("MM/dd/yyyy", new CultureInfo("en"));
        StateHasChanged();
        Value = dateTime;
        ValueChanged.InvokeAsync(dateTime);
    }



    IEnumerable<CalendarItem> CreateCalendar()
    {
        int year = _chooseYear;
        
        int monthNumber = _chooseMonth;
        DateTime dateTime = new (year, monthNumber, 1);

        int daysInMonth = DateTimeFormatInfo.InvariantInfo.Calendar.GetDaysInMonth(year, _chooseMonth);
        
        int prevMonthNumber = _chooseMonth != 1 ? _chooseMonth -1 : 12;
        int nextMonthNumber = _chooseMonth != 12 ? _chooseMonth + 1 : 1;
        
        int daysInPrevMonth =
            DateTimeFormatInfo.InvariantInfo.Calendar.GetDaysInMonth(year, prevMonthNumber);
        
        DayOfWeek firstDayInChooseMonth = dateTime.DayOfWeek;
        int dayNumber = _dayNames.ToList()
            .IndexOf(DateTimeFormatInfo.InvariantInfo.GetAbbreviatedDayName(firstDayInChooseMonth));
        
        dateTime = new (year, monthNumber, daysInMonth);
        DayOfWeek lastDayInChooseMonth = dateTime.DayOfWeek;
        int dayNumberLastDay = _dayNames.ToList()
            .IndexOf(DateTimeFormatInfo.InvariantInfo.GetAbbreviatedDayName(lastDayInChooseMonth));

        for (int a = dayNumber % 7; a > 0; a--)
        {
            if (Value is not null)
            {
                int day = daysInPrevMonth - a + 1;
                if (day == Value.Value.Day && Value.Value.Month == prevMonthNumber && Value.Value.Year == year)
                {
                    yield return new CalendarItem(day, prevMonthNumber, true);
                    continue;
                }
            }
            yield return new CalendarItem(daysInPrevMonth - a + 1, prevMonthNumber , false);
        }

        for (byte a = 1; a < daysInMonth +1 ; a++)
        {
            if (Value is not null)
            {
                if (a == Value.Value.Day && Value.Value.Month == monthNumber && Value.Value.Year == year)
                {
                    yield return new CalendarItem(a, _chooseMonth, true);
                    continue;
                }
            }
            yield return new CalendarItem(a, _chooseMonth, false);
        }

        for (byte a = 1; a < 7 - dayNumberLastDay + 7; a++)
        {
            if (Value is not null)
            {
                if (a == Value.Value.Day && Value.Value.Month == nextMonthNumber && Value.Value.Year == year)
                {
                    yield return new CalendarItem(a, nextMonthNumber, true);
                    continue;
                }
            }
            yield return new CalendarItem(a, nextMonthNumber, false);
        }
    }

    void SelectDateOnPicker(int dayNumber, int month)
    {
        int year = _chooseYear;
        switch (month)
        {
            case 1 when _chooseMonth == 12:
                year += 1;
                break;
            case 12 when _chooseMonth == 1:
                year -= 1;
                break;
        }
        
        DateTime dateTime = new (year, month, dayNumber);
        
        InternalValue = dateTime.ToString("MM/dd/yyyy", new CultureInfo("en"));
        _chooseMonth = month;
        _chooseYear = year;
        StateHasChanged();
        Value = dateTime;
        ValueChanged.InvokeAsync(dateTime);
    }

    bool _datePickerShow = false;
    bool _mouseOver = false;
    bool _withFocus = false;
    void DatePickerShow()
    {
        _datePickerShow = true;
        _withFocus = true;
        StateHasChanged();
    }
    void DatePickerHide()
    {
        _withFocus = false;
        
        if (_mouseOver) return;
        
        _datePickerShow = false;
        StateHasChanged();
    }

    void MouseEnter()
    {
        _mouseOver = true;
        StateHasChanged();
    }

    void MouseLeave()
    {
        _mouseOver = false;
        if (_withFocus) return;
        _datePickerShow = false;
        StateHasChanged();
    }
    
    
    void RemoveOneYear()
    {
        _chooseYear -= 1;
        StateHasChanged();
    }
    void AddOneYear()
    {
        if (_chooseYear <= 0) return;
        
        _chooseYear += 1;
        StateHasChanged();
    }
    void AddOneMonth()
    {
        if (_chooseMonth == 12)
        {
            _chooseMonth = 1;
            AddOneYear();
        }
        else
        {
            _chooseMonth += 1;
            StateHasChanged();
        }
    }
    void RemoveOneMonth()
    {
        if (_chooseMonth == 1)
        {
            _chooseMonth = 12;
            RemoveOneYear();
        }
        else
        {
            _chooseMonth -= 1;
            StateHasChanged();
        }
    }
}

class CalendarItem
{
    public int Value { get; set; }
    public int Month { get; set; }
    public bool IsSelect { get; set; }

    public CalendarItem(int value, int month, bool isSelect)
    {
        Value = value;
        IsSelect = isSelect;
        Month = month;
    }
}

[EventHandler("onmouseleave", typeof(MouseEventArgs), true, true)]
[EventHandler("onmouseenter", typeof(MouseEventArgs), true, true)]
public static class EventHandlers
{
}