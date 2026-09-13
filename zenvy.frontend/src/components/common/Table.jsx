import { DataTable } from 'react-native-paper';

export const Table = ({ data, columns }) => {
  return (
    <DataTable>
      <DataTable.Header>
        {columns.map((column) => (
          <DataTable.Title key={column.key}>{column.label}</DataTable.Title>
        ))}
      </DataTable.Header>
      {data.map((row, index) => (
        <DataTable.Row key={index}>
          {columns.map((column) => (
            <DataTable.Cell key={column.key}>{row[column.key]}</DataTable.Cell>
          ))}
        </DataTable.Row>
      ))}
    </DataTable>
  );
};