using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DesignLibrary.Components.MInputNumber;

public partial class MInputNumber: ComponentBase
{
    [Inject] IJSRuntime JsRuntime { get; set; }
    
    [Parameter] public string Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string Placeholder { get; set; }
    [Parameter] public string Style { get; set; }
    [Parameter] public byte MaxLength { get; set; } = 100;
    [Parameter] public string Format { get; set; }

    Guid _idInput = Guid.NewGuid();
    [JSInvokable("OnChangeInternal")]
    public async void OnChangeInternal(ChangeEventArgs args, bool paste = false)
    {
        StringBuilder value = new($"{args.Value}");

        Value ??= "";
        if (paste)
        {
            value = new StringBuilder(Regex.Replace($"{value}", "[^0-9]", ""));
        }
        
        int positionCaret = await JsRuntime.InvokeAsync<int>("getCaret", _idInput);
        
        if (Format is not null)
        {
            if (Value.Length > value.Length && !paste)
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
                    Value = Value.Replace($"{divider}", "");
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
        Value = newValue;
        StateHasChanged();
        await ValueChanged.InvokeAsync(newValue);

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
            await JsRuntime.InvokeVoidAsync("setCaret", _idInput, positionCaret);
        }
    }
    
    List<char> _dividers = new ();
    List<byte> _countNumber = new();
    List<(byte, byte)> _moreThenOneDivider = new();

    protected override void OnParametersSet()
    {
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
            DotNetObjectReference<MInputNumber> dotNetHelper = DotNetObjectReference.Create(this);
            JsRuntime.InvokeVoidAsync("addPasteLogic", _idInput,dotNetHelper);
            JsRuntime.InvokeVoidAsync("addBackspaceKeyPress", _idInput, _countNumber);
        }
    }
}