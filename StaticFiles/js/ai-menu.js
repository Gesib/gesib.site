const API_URL = 'https://gesib.site';

var githubLogin = document.getElementById("github-login-overlay");
githubLogin.setAttribute("asp-action", "Login");
githubLogin.setAttribute("asp-controller", "Auth");

document.getElementById("chatInput").addEventListener("keydown", function (event) {
	if (event.key === 'Enter') {
		sendMessage();
	}
});

// Check if user is authenticated and show/hide comment box
function handleNotAuthenticated() {
    $("#github-login-overlay").show();
    $(".comment_button").hide();
    $("#comment_input").hide();

    $("#github-login-overlay").click(function () {
		window.location.href = `${API_URL}/login?returnUrl=${API_URL}/portfolio.html`;
    });
}

fetchAPI('isAuthenticated', 'GET', null,
	response => {
		response.json().then(isAuthenticated => {
			if (isAuthenticated) {
				$("comment_button").show();
				$("#comment_input").show();
				$("#github-login-overlay").hide();
			} else {
				handleNotAuthenticated();
			}
		});
	},
	error => handleNotAuthenticated()
);

const commentButton = document.getElementById("comment_button");
commentButton.addEventListener("click", function (event) {
	event.preventDefault();
	submitComment();
});

document.addEventListener('DOMContentLoaded', function () {
	getComments();
});

function getComments() {
	fetchAPI('comments', 'GET', null,
		response => {
			response.json().then(comments => {
				displayComments(comments);
			});
		},
		error => console.error('Error retrieving comments', error)
	);
}

function displayComments(comments) {
	const commentBox = document.getElementById("comment_list");
	commentBox.innerHTML = "";

	comments.forEach(commentData => {
		const comment = commentData.Key;
		const isOwnComment = commentData.Value;
		const newComment = createCommentElement(comment, isOwnComment);
		commentBox.prepend(newComment);
	});
}

function submitComment() {
	const text = document.getElementById("comment_input").value;
	if (!validateComment(text)) {
		return;
	}
	fetchAPI("comments", "POST", text,
		response => {
			response.json().then(comment => {
				const commentBox = document.getElementById("comment_list");
				const newComment = createCommentElement(comment, true);
				commentBox.insertBefore(newComment, commentBox.firstChild);
				document.getElementById("comment_input").value = "";
			});
		},
		error => console.error(error),
		() => { },
		"text/plain"
	);
}

function editComment(commentElement) {
	const textElement = commentElement.querySelector(".text");
	const originalText = textElement.textContent;
	const editButton = commentElement.querySelector(".edit-comment");

	// Replace the text with a textarea for editing
	const textarea = document.createElement("textarea");
	textarea.classList.add("edit-comment-textarea");
	textarea.value = originalText;
	textElement.replaceWith(textarea);

	// Change the edit button to a save button
	editButton.textContent = "Save";
	editButton.onclick = function () {
		if (validateComment(textarea.value)) {
			saveComment(commentElement, textarea.value);
		}
	};
}

function saveComment(commentElement, newText) {
	const comment = commentElement.comment;
	const editButton = commentElement.querySelector(".edit-comment");
	const deleteButton = commentElement.querySelector(".delete-comment");

	comment.Text = newText;

	fetchAPI(`editComment/${comment.Id}`, 'PUT', newText,
		() => {
			const textElement = document.createElement("p");
			textElement.classList.add("text");
			textElement.textContent = newText;
			const textarea = commentElement.querySelector(".edit-comment-textarea");
			textarea.replaceWith(textElement);
			editButton.textContent = "Edit";
			editButton.onclick = function () {
				editComment(commentElement);
			};
			deleteButton.disabled = false;
		},
		error => console.error(error),
		() => { },
		'text/plain'
	);
}

function validateComment(commentText) {
	if (commentText.trim() === '') {
		alert('Comment cannot be blank!');
		return false;
	}
	if (commentText.length > 3000) {
		alert('Comment cannot be longer than 3000 characters!');
		return false;
	}
	return true;
}

function deleteComment(comment) {
	const confirmed = window.confirm("Are you sure you want to delete this comment?");

	if (confirmed) {
		fetchAPI(`deleteComment/${comment.Id}`, 'DELETE', null, 
			() => {
				const commentElement = document.querySelector(`[data--id="${comment.Id}"]`);
				if (commentElement) {
					commentElement.remove();
				} else {
					console.warn("Comment element not found.");
				}
			},
		);
	}
}


function createCommentElement(comment, isOwnComment) {
	const newComment = document.createElement("div");
	newComment.dataset.Id = comment.Id;
	newComment.classList.add("comment");
	newComment.innerHTML = `
	<p class="author">${comment.UserDisplayname} <span class="time">${getTimeAgo(comment.Time)}</span></p>
	<p class="text">${comment.Text}</p>`;

	if (isOwnComment) {
		const editButton = document.createElement("button");
		editButton.textContent = "Edit";
		editButton.classList.add("edit-comment");
		editButton.onclick = function () {
			editComment(newComment);
		};

		const deleteButton = document.createElement("button");
		deleteButton.textContent = "Delete";
		deleteButton.classList.add("delete-comment");
		deleteButton.onclick = function () {
			deleteComment(comment);
		};

		newComment.appendChild(editButton);
		newComment.appendChild(deleteButton);
	}

	newComment.comment = comment;

	return newComment;
}

function getTimeAgo(timestamp) {
	const now = new Date();
	const timeDiff = now - new Date(timestamp);
	const seconds = Math.floor(timeDiff / 1000);
	const minutes = Math.floor(seconds / 60);
	const hours = Math.floor(minutes / 60);
	const days = Math.floor(hours / 24);
	const months = Math.floor(days / 30);
	const years = Math.floor(months / 12);

	if (seconds < 1) {
		return `just now`;
	} else if (seconds < 60) {
		return `${seconds} second${seconds > 1 ? 's' : ''} ago`;
	} else if (minutes < 60) {
		return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
	} else if (hours < 24) {
		return `${hours} hour${hours > 1 ? 's' : ''} ago`;
	} else if (days === 1) {
		return `yesterday`;
	} else if (days < 30) {
		return `${days} days ago`;
	} else if (months === 1) {
		return `1 month ago`;
	} else if (months < 12) {
		return `${months} months ago`;
	} else {
		return `${years} year${years > 1 ? 's' : ''} ago`;
	}
}


function clearConversation() {
	const chatHistory = document.getElementById("chat_history");
	chatHistory.innerHTML = "";
	fetchAPI('clear', 'POST', null);
}

function submitOnEnter(event) {
	if (event.keyCode === 'Enter') {
		sendMessage();
	}
}

function sendMessage() {
	const chatInput = document.querySelector('.chat_input');
	const inputField = document.querySelector('.chat_input');
	const sendButton = document.querySelector('.send_button');
	const message = chatInput.value;
	if (message.trim() !== '') {
		chatInput.value = '';
		const userMessage = document.createElement('div');
		userMessage.className = 'user_message';
		userMessage.innerHTML = '<p>' + message + '</p>';
		const chatHistory = document.querySelector('#chat_history');
		chatHistory.appendChild(userMessage);

		inputField.disabled = true;
		sendButton.classList.add('loading');

		fetchAPI('ask', 'POST', message,
			response => {
				response.text().then(data => {
					CreateBotMessage(data, chatHistory);
				});
			},
			error => CreateBotMessage('Something went wrong, try again (later)', chatHistory),
			() => {
				inputField.value = '';
				inputField.disabled = false;
				sendButton.classList.remove('loading');
			},
			'text/plain'
		);
	}
}

function CreateBotMessage(data, chatHistory) {
	const botMessage = document.createElement('div');
	botMessage.className = 'bot_message';
	botMessage.innerHTML = '<p>' + data + '</p>';
	chatHistory.appendChild(botMessage);
	chatHistory.scrollTo(0, chatHistory.scrollHeight);
}

function fetchAPI(endpoint, method, body = null,
	successCallback = null,
	failureCallback = (error) => console.error(error),
	finalCallback = () => { },
	contentType = 'application/json'
) {
	const url = `${API_URL}/${endpoint}`;

	let options = {
		method: method,
		headers: {
			'Content-Type': contentType,
		},
	};

	if (body) {
		options.body = body
	}

	return fetch(url, options)
		.then(response => {
			if (response.ok) {
				if (successCallback) {
					successCallback(response);
				}
			} else {
				throw new Error(`Request failed: ${response.statusText}`);
			}
		})
		.catch(failureCallback)
		.finally(finalCallback);
}