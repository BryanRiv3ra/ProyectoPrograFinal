import os
from datetime import datetime

class EventLogger:
    def __init__(self, log_file='data/event_log.txt'):
        self.log_file = log_file
        self._ensure_log_folder_exists()

    def _ensure_log_folder_exists(self):
        """Crea la carpeta 'data' si no existe."""
        folder = os.path.dirname(self.log_file)
        if not os.path.exists(folder):
            os.makedirs(folder)

    def log_event(self, event_description):
        """Registra un evento en el archivo de log."""
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        log_entry = f"[{timestamp}] {event_description}\n"
        with open(self.log_file, 'a') as log:
            log.write(log_entry)
        print(f"Evento registrado: {event_description}")

    def get_logs(self):
        with open(self.log_file, 'r') as file:
            return file.readlines()