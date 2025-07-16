document.addEventListener("DOMContentLoaded", function () {
    const audio = document.getElementById("audioPlayer");
    const playBtn = document.getElementById("playBtn");
    const progressBar = document.getElementById("progressBar");
    const timeDisplay = document.getElementById("timeDisplay");
    const likeButton = document.getElementById("likeButton");
    const commentForm = document.getElementById("commentForm");
    const deleteForms = document.querySelectorAll(".delete-comment-form");
    const likeCountSpan = document.getElementById("likeCount");

    // Format helper
    function formatTime(seconds) {
        if (isNaN(seconds)) return "0:00";
        const minutes = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${minutes}:${secs < 10 ? "0" : ""}${secs}`;
    }

    function formatLikeCount(count) {
        if (count >= 1_000_000) return (count / 1_000_000).toFixed(1).replace(/\.0$/, "") + "M";
        if (count >= 1_000) return (count / 1_000).toFixed(1).replace(/\.0$/, "") + "k";
        return count.toString();
    }

    function updateTimeDisplay() {
        const current = formatTime(audio.currentTime);
        const total = formatTime(audio.duration);
        timeDisplay.innerText = `${current} / ${total}`;
        progressBar.value = Math.floor(audio.currentTime);
    }

    audio.addEventListener("loadedmetadata", function () {
        progressBar.max = Math.floor(audio.duration);
        updateTimeDisplay();
    });

    audio.addEventListener("timeupdate", updateTimeDisplay);

    progressBar.addEventListener("input", function () {
        audio.currentTime = progressBar.value;
    });

    playBtn.addEventListener("click", function () {
        if (audio.paused) {
            audio.play();
            playBtn.innerText = "Pause";
        } else {
            audio.pause();
            playBtn.innerText = "Play";
        }
    });

    // 💖 Like Button Handler
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

                if (likeCountSpan && data.likeCount !== undefined) {
                    likeCountSpan.textContent = formatLikeCount(data.likeCount);
                    likeCountSpan.classList.add("pulse");
                    setTimeout(() => likeCountSpan.classList.remove("pulse"), 300);
                }
            })
            .catch(console.error);
    });

    // 📝 Comment Submission
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

    // ❌ Delete Comment
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
