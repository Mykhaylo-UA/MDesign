window.getCaret = (el) =>{
    return document.getElementById(el).selectionStart ? document.getElementById(el).selectionStart : 0;
}
window.setCaret = (el, index) =>{
    return document.getElementById(el).setSelectionRange(index, index);
}
window.document.addEventListener("keydown", e=>{
    let numberCode = (e.keyCode === 8 ||
        e.ctrlKey ||
        e.keyCode === 9 ||
        e.keyCode === 46 ||
        (e.keyCode >= 48 && e.keyCode <= 57) ||
        (e.keyCode >= 37 && e.keyCode <= 40) ||
        (e.keyCode >= 96 && e.keyCode <= 105))
    
    if(e.target.classList.contains("numberOnly")){
        if (!numberCode)
        {
            e.preventDefault();
        }
    }
});
window.addBackspaceKeyPress = (id, dividerPosition) =>{
    let input = document.getElementById(id);
    input.addEventListener("keydown", (e) =>{
        if(input.value.length !== 1){
            if(e.keyCode === 8 || e.keyCode === 46){
                let positionCaret = getCaret(id);
                dividerPosition.forEach(d =>{
                        if(positionCaret === d+1 &&
                            positionCaret !== input.value.length){
                            e.preventDefault();
                        }
                    });
            }
        }
    });
}
window.addPasteLogic = (id, dotNetHelper) =>{
    let input = document.getElementById(id);
    input.addEventListener("paste", (e) =>{
        e.preventDefault();
        let value;
        
        let copyValue = (e.clipboardData || window.clipboardData).getData('text');
        copyValue = copyValue.replace(/[^\d.-]/g, '');
        
        let range = document.getSelection();
        
        if(range.toString().length === e.target.value.length){
            value = copyValue;
        }
        else{
            let rangeString = range.toString()
            let after = e.target.value.slice(0, input.selectionStart);
            let before = e.target.value.slice(input.selectionStart + rangeString.length);
            console.log(rangeString.length);
            value = after + copyValue + before;
        }
        
        return dotNetHelper.invokeMethodAsync('OnChangeInternal', {Value: value}, true);
    });
    input.addEventListener("drop", e=> e.preventDefault());
}
