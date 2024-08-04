const toolbarOptions = [
    [{ 'size': ['normal', 'large', 'huge'] }],
    [{ 'header': 1 }, { 'header': 2 }],
    ['bold', 'italic'],
    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
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