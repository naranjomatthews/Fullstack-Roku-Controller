// We can access window.CrComLib because the vite-plugin-static-copy package configured 
// in the vite.config.js file copied over the Crestron lib files to the build output directory.
// Specifically it copied over the UMD files, which bind to the window automatically, hence why
// we can access window.CrComLib here just fine without importing or manually binding.

// Initialize eruda for panel/app debugging capabilities (in dev mode only)
if (import.meta.env.VITE_APP_ENV === 'development') {
    import('eruda').then(({ default: eruda }) => {
        eruda.init();
    });
}


document.addEventListener('DOMContentLoaded', () => {

    // Common SIMPL# state subscriptions that work across both pages
    setupCommonSubscriptions();
    
    // Check which page we're on and initialize accordingly
    if (document.getElementById('IP_Address_Label')) {
        // We're on the main index.html page
        initializeMainPage();
    }
    
    if (document.getElementById('Textinput_IP_Address')) {
        // We're on the settings.html page
        initializeSettingsPage();
    }
});

function setupCommonSubscriptions() {
    // IP Address display - works on main page
    window.CrComLib.subscribeState('s', '2', (value) => {
        const elem = document.getElementById("IP_Address_Label");
        if (elem) {
            elem.value = value;
        }
    });

    // Shortcut names - works on main page
    window.CrComLib.subscribeState('s', '40', (value) => {
        const elem = document.getElementById("Shortcut_Name_1");
        if (elem) {
            elem.value = value;
        }
    });

    window.CrComLib.subscribeState('s', '41', (value) => {
        const elem = document.getElementById("Shortcut_Name_2");
        if (elem) {
            elem.value = value;
        }
    });

    window.CrComLib.subscribeState('s', '42', (value) => {
        const elem = document.getElementById("Shortcut_Name_3");
        if (elem) {
            elem.value = value;
        }
    });

    // Logger subscriptions - works on settings page
    const loggerMappings = [
        { signal: '8', elementId: 'Logger_Text_Display_1' },
        { signal: '9', elementId: 'Logger_Text_Display_2' },
        { signal: '10', elementId: 'Logger_Text_Display_3' },
        { signal: '11', elementId: 'Logger_Text_Display_4' },
        { signal: '12', elementId: 'Logger_Text_Display_5' },
        { signal: '13', elementId: 'Logger_Text_Display_6' },
        { signal: '14', elementId: 'Logger_Text_Display_7' },
        { signal: '15', elementId: 'Logger_Text_Display_8' },
        { signal: '16', elementId: 'Logger_Text_Display_9' },
        { signal: '17', elementId: 'Logger_Text_Display_10' }
    ];

    loggerMappings.forEach(mapping => {
        window.CrComLib.subscribeState('s', mapping.signal, (value) => {
            const elem = document.getElementById(mapping.elementId);
            if (elem) {
                elem.value = value;
            }
        });
    });

    // Theme selector - works on settings page
    const themeSelect = document.getElementById("themeSelect");
    if (themeSelect) {
        themeSelect.addEventListener("change", function () {
            const selectedTheme = this.value;
            document.body.classList.remove("light-mode", "dark-mode");
            document.body.classList.add(selectedTheme + "-mode");
        });
    }
}

function initializeMainPage() {
    // D-pad controls
    const dpadButtons = [
        { id: 'Up', signal: 23 },
        { id: 'Down', signal: 24 },
        { id: 'Left', signal: 25 },
        { id: 'Right', signal: 26 },
        { id: 'Select', signal: 27 }
    ];

    dpadButtons.forEach(button => {
        const elem = document.getElementById(button.id);
        if (elem) {
            elem.addEventListener('click', () => {
                window.CrComLib.publishEvent('b', button.signal, true);
                setTimeout(() => window.CrComLib.publishEvent('b', button.signal, false), 100);
            });
        }
    });

    // Media control buttons
    const mediaButtons = [
        { id: 'Rewind_Button', signal: 16 },
        { id: 'Pause_Button', signal: 17 },
        { id: 'Forward_Button', signal: 18 },
        { id: 'Info_Button', signal: 19 },
        { id: 'Instant_Replay_Button', signal: 20 },
        { id: 'Back_Button', signal: 21 },
        { id: 'Home_Button', signal: 22 }
    ];

    mediaButtons.forEach(button => {
        const elem = document.getElementById(button.id);
        if (elem) {
            elem.addEventListener('click', () => {
                window.CrComLib.publishEvent('b', button.signal, true);
                setTimeout(() => window.CrComLib.publishEvent('b', button.signal, false), 100);
            });
        }
    });

    // Shortcut buttons
    const shortcutButtons = [
        { id: 'Shortcut_Button_1', signal: 28 },
        { id: 'Shortcut_Button_2', signal: 29 },
        { id: 'Shortcut_Button_3', signal: 30 }
    ];

    shortcutButtons.forEach(button => {
        const elem = document.getElementById(button.id);
        if (elem) {
            elem.addEventListener('click', () => {
                window.CrComLib.publishEvent('b', button.signal, true);
                setTimeout(() => window.CrComLib.publishEvent('b', button.signal, false), 100);
            });
        }
    });

    // Language display - works on main page
    window.CrComLib.subscribeState('s', '3', (value) => {
        const elem = document.getElementById("Language_Label");
        if (elem) {
            elem.value = value;
        }
    });
 

    /*
    // Commented out icon URL subscriptions as in original
    window.CrComLib.subscribeState('s', '4', (url) => {
      document.getElementById("Tubi_Icon").src = url;
    });

    window.CrComLib.subscribeState('s', '5', (url) => {
      document.getElementById("YouTube_Icon").src = url;
    });

    window.CrComLib.subscribeState('s', '6', (url) => {
      document.getElementById("Fish_Icon").src = url;
    });
    */
}

function initializeSettingsPage() {
    // IP Address input handling
    const ipInput = document.getElementById('Textinput_IP_Address');
    if (ipInput) {
        ipInput.addEventListener('input', () => {
            const Roku_IP = ipInput.value;
            window.CrComLib.publishEvent('s', 1, Roku_IP); // Send updated IP on every change
        });
    }

    // Enter button
    const enterButton = document.getElementById('Enter_Button');
    if (enterButton) {
        enterButton.addEventListener('click', () => {
            window.CrComLib.publishEvent('b', 31, true);
            setTimeout(() => window.CrComLib.publishEvent('b', 31, false), 100);
        });
    }
}


