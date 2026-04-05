import subprocess
import platform
import sys
import shutil
import time

def check_dependencies():
    # check if docker is installed
    if not shutil.which("docker"):
        print("Error: Docker is not installed")
        sys.exit(1)

def get_linux_terminal():
    # common Linux terminals, if your terminal is not found append it to this list
    terminals = ["gnome-terminal", "konsole", "xfce4-terminal", "xterm", "kitty", "alacritty"]
    for t in terminals:
        if shutil.which(t):
            return t
    return None

def start_server():
    print("Start madn server via docker compose")
    subprocess.Popen(["docker", "compose", "up", "-d"])

def start_clients(count):
    os_type = platform.system()

    for i in range(count):
        if os_type == "Windows":
            subprocess.Popen(["start", "cmd", "/k", "MadnClient"], shell=True)
        else:
            terminal = get_linux_terminal()
            subprocess.Popen([terminal, "-e", "./MadnClient"])

if __name__ == "__main__":
    check_dependencies()

    num_clients = int(input("Anzahl der Clients: "))

    start_server()
    time.sleep(1)
    start_clients(num_clients)
    print("Setup abgeschlossen.")
