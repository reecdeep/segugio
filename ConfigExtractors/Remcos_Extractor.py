# -*- coding: utf-8 -*-
__author__ = "reecdeep"
__version__ = "1.0"
__script_name__ = "Remcos"

import sys

def read_memory_dump(file_path):
    with open(file_path, 'rb') as file:
        return file.read()

def find_portion_with_micrecords(memory_dump):
    target_string = "19700101000000Z".encode()
    start_index = 0
    while True:
        target_index = memory_dump.find(target_string, start_index)
        if target_index == -1:
            break

        null_sequence_index = memory_dump.rfind(b'\x00\x00\x00\x00\x00', 0, target_index)
        if null_sequence_index == -1:
            break

        extracted_memory = memory_dump[null_sequence_index:target_index]

        if b"MicRecords" in extracted_memory:
            try:
                # Stampa l'output direttamente in byte sulla console
                sys.stdout.buffer.write(extracted_memory)
                sys.stdout.buffer.write(b'\n')  # Aggiunge una nuova linea
            except Exception as e:
                print("Errore durante la scrittura dell'output:", e)
            break
        else:
            start_index = target_index + 1

# Dato che il vero dump di memoria non è stato processato correttamente, questa funzione non verrà eseguita.
# Per utilizzarla, dovrai passare il dump di memoria come una stringa esadecimale.


if __name__ == '__main__':
    if len(sys.argv) != 2:
        print("Usage: python script.py <path_to_memory_dump>")
        sys.exit(1)
    
    file_path = sys.argv[1]
    memory_dump = read_memory_dump(file_path)
    find_portion_with_micrecords(memory_dump)
    
    
    
    
    