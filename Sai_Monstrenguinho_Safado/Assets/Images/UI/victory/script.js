const clickSound = document.getElementById('click-sound');

document.querySelectorAll('#main-menu button').forEach(button => {
    button.addEventListener('click', () => {
        // Play sound
        if (clickSound) {
            clickSound.currentTime = 0;
            clickSound.play();
        }

        // Add a 'clicked' class for visual feedback
        button.classList.add('clicked');

        // Optional: Do something based on which button was clicked
        const buttonId = button.id;
        setTimeout(() => {
            switch (buttonId) {
                case 'start-btn':
                    alert("Starting the game!");
                    break;
                case 'options-btn':
                    alert("Here are the options!");
                    break;
                case 'credits-btn':
                    alert("Made with love by an AI!");
                    break;
            }
             button.classList.remove('clicked'); // Reset visual state
        }, 150); // Delay to allow sound to play and animation to be seen
    });
});