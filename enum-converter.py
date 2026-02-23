import glob
import json
import os

DEPTH_TO_BIT_SHIFT: dict[int, int] = {
    0: 24,
    1: 16,
    2: 8,
    3: 0,
}

def retrieve_all_json_files() -> dict[str, dict[str, object]]:
    current_working_directory: str = os.getcwd()
    # Find all files in the folder that end with '.json'
    # Use os.path.join to create the search pattern correctly
    search_pattern: str = os.path.join(current_working_directory, 'raw', 'json', '*.json')
    json_files: list[str] = glob.glob(search_pattern)

    main_type_to_sub_types: dict[str, dict[str, object]] = {}
    # Loop through each found JSON file
    for file_path in json_files:
        # Open each file in read mode using a context manager
        with open(file_path, 'r') as f:
            # Load the JSON data from the file into a Python dictionary/list
            data: dict[str, object] = json.load(f)
            
            # Extract the main type (file name without extension) and sub types
            file_name: str = os.path.basename(file_path)
            main_type: str = os.path.splitext(file_name)[0]
            
            # Store the data in main_type_to_sub_types dictionary
            main_type_to_sub_types[main_type] = data

    return main_type_to_sub_types

def build_c_sharp_enum(main_type: str, sub_types: dict[str, object], depth: int) -> str | None:
    if depth > 3 or sub_types is None or len(sub_types.items()) == 0:
        return None
    
    enum_file: str = """// Generated Code - Do Not Edit //
namespace HeartAndHavoc_GoblinLegacy.Enumeration.Generated
{
    public enum {main_type}
    {
{sub_types}
    }
}
"""

    enum_file = enum_file.replace('{main_type}', main_type.capitalize())

    index: int = 0b0001
    sub_types_to_enum_values: dict[str, int] = {}
    for sub_type, sub_sub_types in sub_types.items():
        sub_types_to_enum_values[sub_type.upper()] = index << DEPTH_TO_BIT_SHIFT[depth]
        index += 1
        
        print_enum_file(sub_type, build_c_sharp_enum(sub_type, sub_sub_types, depth + 1))

    enum_file = enum_file.replace('{sub_types}', '\n'.join([f'        {sub_type} = {enum_value},' for sub_type, enum_value in sub_types_to_enum_values.items()]))
    return enum_file

def print_enum_file(main_type: str, enum_file: str | None) -> None:
    if enum_file is None:
        return

    current_working_directory: str = os.getcwd()
    file_name: str = os.path.join(current_working_directory, 'Enumeration', 'Generated', f'{main_type.capitalize()}.cs')
   
    with open(file_name, 'w') as f:
        f.write(enum_file)
            
def main():
    main_type_to_sub_types: dict[str, dict[str, object]] = { 'baseType': retrieve_all_json_files() }

    for main_type, sub_types in main_type_to_sub_types.items():
        print_enum_file(main_type, build_c_sharp_enum(main_type, sub_types, 0))

main()