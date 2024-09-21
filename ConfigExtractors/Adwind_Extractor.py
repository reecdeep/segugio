# -*- coding: utf-8 -*-
__author__ = "reecdeep"
__version__ = "1.0"
__script_name__ = "Adwind_RAT"

import sys

def estrai_contenuto(file_path):
    # Leggi il contenuto del file .dmp
    with open(file_path, 'rb') as file:
        byte_content = file.read()

    # Converte i byte in stringa esadecimale
    hex_dump = byte_content.hex()

    # Cerca le posizioni di inizio e fine delle stringhe target
    start_marker = "3c70726f706572746965733e" # <properties> in esadecimale
    end_marker = "3c2f70726f706572746965733e" # </properties> in esadecimale

    start_pos = hex_dump.find(start_marker) + len(start_marker)
    end_pos = hex_dump.find(end_marker)

    # Se entrambe le stringhe target sono state trovate, estrai il contenuto
    if start_pos > len(start_marker) and end_pos > start_pos:
        # Estrai il contenuto e convertilo da esadecimale a stringa
        extracted_hex = hex_dump[start_pos:end_pos]
        extracted_bytes = bytes.fromhex(extracted_hex)
        extracted_content = extracted_bytes.decode('utf-8')

        print("Contenuto estratto:")
        print(extracted_content)
    


if __name__ == '__main__':
    if len(sys.argv) != 2:
        print("Usage: python script.py <path_to_memory_dump>")
        sys.exit(1)
    
    file_path = sys.argv[1]
    estrai_contenuto(file_path)
  
    