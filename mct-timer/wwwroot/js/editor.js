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

// Initialize Quill as soon as the script loads
// Quill should be available since it's loaded before this script in the layout
if (typeof Quill !== 'undefined') {
    console.log('Quill is available, initializing editor');
    try {
        window.quill = new Quill('#editor', options);
        console.log('Quill editor initialized successfully');
    } catch (error) {
        console.error('Error initializing Quill:', error);
    }
} else {
    console.error('Quill is not defined when editor.js loaded');
}