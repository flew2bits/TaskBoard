$(() => {
    $("input[name='priority']").on('change', (evt) => {
        $(evt.target).closest("form").trigger("submit");
    })
    
    $('.edit-title').on('click', (evt) => {
        let $target = $(evt.target);
        let card = $target.closest('card');
        let title = $target.closest('.card-title');
        let form = $target.closest('.card-body').find('.edit-title-form');
        
        form.removeClass('d-none');
        form.find('[name="title"]').val(card.data("title"));
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
    
    let connection = 
        new signalR.HubConnectionBuilder()
            .withUrl("/taskHub")
            .withAutomaticReconnect()
            .build();
    
    connection.on("ErrorMessage", (message) => toastr.error(message));
    
    connection.on("RemoveTask", taskId => $(`[data-task-id="${taskId}"]`).remove());
    
    connection.on("StateChanged", (taskId, state) => {
        let task = $(`[data-task-id="${taskId}"]`);
        task.remove();
        let target = $(`[data-state="${state}"]`);
        target.append(task);
        
        let actionsDiv = task.find(".task-actions");
        actionsDiv.empty();
        let actions = target.data('actions').split(',');
        
        for (let action of actions) {
            let btnStyle = 'primary';
            if (action[0] === '!') {
                btnStyle = "danger";
                action = action.slice(1);
            } 
            actionsDiv.append(`<a href="#" class="btn btn-${btnStyle} me-1" data-action="${action}">${action}</a>`)
        }
    })
    
    connection.on("Unassigned", taskId => {
       let assignmentSelect = $(`[data-task-id="${taskId}"]`).find('select[name="taskAssignee"]');
       assignmentSelect.val('00000000-0000-0000-0000-000000000000');
    });
    
    connection.on("Reassigned", (taskId, userId) => {
        let assignmentSelect = $(`[data-task-id="${taskId}"]`).find('select[name="taskAssignee"]');
        assignmentSelect.val(userId);
    });
    
    connection.on("TaskCreated", taskId => {
        $.ajax({
            url: `/api/${taskId}`,
            method: "get"
        }).then(data => {
            console.dir(data);
            let $template = $('#newTaskTemplate');
            let clone = $($template.html());
            clone.attr('data-task-id', taskId);
            close.attr('data-title', data.title);
            clone.find('.card-title .title-text').text(data.title);
            clone.find('input[name="title"]').val(data.title);
            clone.find(`#priority-${data.priority}`.toLowerCase()).prop("checked", true);
            
            for (let node of clone.find('input[name="priority"]')) {
                $(node).attr("id", `${taskId}-${node.id}`).attr("name", `${taskId}-priority`);
            }
            for (let node of clone.find('label[for^="priority-"]')) {
                $(node).attr("for", `${taskId}-${$(node).attr("for")}`);
            }
            
            clone.appendTo("[data-state='New']")
        });
    })
    
    connection.on("UpdatePriority", (taskId, priority) => {
        $(`#${taskId}-priority-${priority}`.toLowerCase()).prop("checked", true);
    })
    
    connection.on("Renamed", (taskId, title) => {
        let card = $(`.card[data-task-id=${taskId}]`);
        card.find('.title-text').text(title);
        card.attr('data-title', title);
        card.find('.edit-title-value').val(title);
    })
    
    connection.start().finally();
    
    let $main = $('main');
    
    $main.on('click', '[data-action]', evt => {
        let $target = $(evt.target);
        let action = $target.data("action");
        let taskId = $target.closest(".card").data('taskId');
        connection.invoke("ChangeTaskState", action, taskId).finally();
    })
    
    $main.on('focus', 'select[name="taskAssignee"]', evt => {
        let $target = $(evt.target);
        $target.data("old-value", $target.find(":checked").val());
    })
    
    $main.on('change', 'select[name="taskAssignee"]', evt => {
        let $target = $(evt.target);
        let option = $target.find(":checked");
        let userId = option.val();
        let taskId = $target.closest('.card').data('taskId');
        connection.invoke("ReassignTask", taskId, userId).then(data => {
            if (!data) $target.val($target.data('old-value'));
        });
        evt.preventDefault();
    })
    
    $main.on('change', 'input[name$="-priority"]', evt => {
        let $target = $(evt.target);
        if (!$target.is(':checked')) return;
        let options = $target.closest('.priority-options');
        let priority = $target.val();
        let taskId = $target.closest('.card').data('taskId');
        connection.invoke("SetPriority", taskId, priority)
            .catch(err => {
                options.find(`input[value="${options.data("option")}`).prop('checked', true);
            })
            .then(_ => options.attr('data-option', priority))
    })

    $main.on('click', '.edit-title-save',evt => {
        let $target = $(evt.target);
        let card = $target.closest('.card');
        let cardTitle = card.find('.card-title');
        let newTitle = card.find('.edit-title-value').val();
        let taskId = card.data('taskId');
        connection.invoke("RenameTask", taskId, newTitle)
            .then(_ => {
                card.data("title", newTitle);
                cardTitle.find('.title-text').text(newTitle);
            })
            .finally(_ => {
                cardTitle.removeClass('d-none');
                card.find('.edit-title-form').addClass('d-none');
            });
        
    })
    
})