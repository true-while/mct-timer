const toolbarOptions = [
    [{ 'header': 1 }, { 'header': 2 }, { 'header': 3 }],
    ['bold', 'italic'],
    [{ 'list': 'bullet' }],
    [ { align: '' }, { align: 'center' }],
    [{ 'color': [] }, { 'background': [] }],

];


const options = {
    debug: 'info',
    modules: {
        toolbar: toolbarOptions
    },
    placeholder: '',
    theme: 'snow'
};
const quill = new Quill('#editor', options);