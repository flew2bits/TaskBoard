$(() => {
    $("input[name='priority']").on('change', (evt) => {
        $(evt.target).closest("form").trigger("submit");
    })
    
    $('.edit-title').on('click', (evt) => {
        let $target = $(evt.target);
        let title = $target.closest('.card-title');
        let form = $target.closest('.card-body').find('.edit-title-form');
        
        form.removeClass('d-none');
        form.find('[name="title"]').val(title.data("title"));
        title.addClass('d-none');
    })
    
    $('.edit-title-close').on('click', evt => {
        let $target =$(evt.target);
        let body = $target.closest('.card-body');
        let title = body.find('.card-title');
        let form = body.find('.edit-title-form');

        form.addClass('d-none');
        title.removeClass('d-none');
    })
    
    $('#notesModal').on('show.bs.modal', evt => {
        let $related = $(evt.relatedTarget);
        let taskId = $related.closest('.card-body').data('taskId');
        
        $('#AddNoteTaskId').val(taskId);
        $('.notes').empty();
        $('#AddNoteInResponseTo').val('00000000-0000-0000-0000-000000000000');
        
        $.ajax({
            url: `/api/${taskId}/notes`,
            method: 'get'
        }).then(data => {
            for (let note of data.notes) {
                $(".notes").append(buildNoteTree(note));
            }
        });
    }).on('click', '.reply-to-note', evt => {
       let $note = $(evt.target).closest('.note');
       if ($note.is('.replying')) {
           $note.removeClass('replying');
           $('#AddNoteInResponseTo').val('00000000-0000-0000-0000-000000000000');
       } else {
           $(".note").removeClass("replying");
           $note.addClass("replying");
           $('#AddNoteInResponseTo').val($note.data("noteId"));
       }
    });
    
    function buildNoteTree(note) {
        let children = note.responses.map(buildNoteTree);
        let responses =$(`<div class="responses"></div>`);
        let node = $(`<div class="mb-2 note" data-note-id="${note.noteId}">${note.text} <i class="fa fa-reply reply-to-note" role="button"></i></div>`);
        responses.append(children);
        node.append(responses);
        return node;
    }
    
})