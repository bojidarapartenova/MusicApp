document.addEventListener("DOMContentLoaded", function () {
    const audio = document.getElementById("audioPlayer");
    const playBtn = document.getElementById("playBtn");
    const progressBar = document.getElementById("progressBar");
    const timeDisplay = document.getElementById("timeDisplay");
    const likeButton = document.getElementById("likeButton");
    const commentForm = document.getElementById("commentForm");
    const deleteForms = document.querySelectorAll(".delete-comment-form");

    // Format helper
    function formatTime(seconds) {
        if (isNaN(seconds)) return "0:00";
        const minutes = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${minutes}:${secs < 10 ? "0" : ""}${secs}`;
    }

    // Update time display
    function updateTimeDisplay() {
        const current = formatTime(audio.currentTime);
        const total = formatTime(audio.duration);
        timeDisplay.innerText = `${current} / ${total}`;
        progressBar.value = Math.floor(audio.currentTime);
    }

    // Load metadata safely
    audio.addEventListener("loadedmetadata", function () {
        progressBar.max = Math.floor(audio.duration);
        updateTimeDisplay();
    });

    // Only update progress bar, no autoplay
    audio.addEventListener("timeupdate", updateTimeDisplay);

    // Allow scrubbing (optional)
    progressBar.addEventListener("input", function () {
        audio.currentTime = progressBar.value;
    });

    // Comment out play/pause for now
    /*
    function togglePlay() {
        if (audio.paused) {
            audio.play();
            playBtn.innerText = "Pause";
        } else {
            audio.pause();
            playBtn.innerText = "Play";
        }
    }

    playBtn.addEventListener("click", togglePlay);
    */

    // Like Button Logic
    likeButton?.addEventListener("click", function () {
        const songId = likeButton.getAttribute("data-song-id");
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch("/Song/Like", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify(songId)
        })
            .then(res => res.json())
            .then(data => {
                if (data.liked) {
                    likeButton.classList.add("liked");
                    likeButton.innerHTML = "♥️";
                } else {
                    likeButton.classList.remove("liked");
                    likeButton.innerHTML = "🤍";
                }
            })
            .catch(console.error);
    });

    // Comment Submission (no reload)
    commentForm?.addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(commentForm);
        const token = formData.get("__RequestVerificationToken");

        fetch(commentForm.action, {
            method: "POST",
            headers: {
                "RequestVerificationToken": token
            },
            body: formData
        })
            .then(response => {
                if (!response.ok) throw new Error("Failed to post comment");
                return response.text();
            })
            .then(() => {
                commentForm.reset();
                location.reload(); // Optional: replace with dynamic comment rendering
            })
            .catch(error => {
                console.error("Error posting comment:", error);
            });
    });

    // Delete Comments
    deleteForms.forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();

            const formData = new FormData(form);
            const token = formData.get("__RequestVerificationToken");

            fetch(form.action, {
                method: "POST",
                headers: {
                    "RequestVerificationToken": token
                },
                body: formData
            })
                .then(response => {
                    if (!response.ok) throw new Error("Failed to delete comment");
                    return response.text();
                })
                .then(() => location.reload())
                .catch(error => console.error("Error deleting comment:", error));
        });
    });
});
