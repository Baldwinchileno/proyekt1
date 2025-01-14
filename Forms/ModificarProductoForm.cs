using AdminSERMAC.Models;
using AdminSERMAC.Services;
using AdminSERMAC.Services.Database;

public class ModificarProductoForm : Form
{
    private TextBox codigoTextBox;
    private TextBox descripcionTextBox;
    private TextBox kilosTextBox;
    private TextBox unidadesTextBox;
    private DateTimePicker fechaVencimientoPicker;
    private Button guardarButton;
    private Button cancelarButton;
    private SQLiteService sqliteService;
    private string codigoOriginal;

    public ModificarProductoForm(
    string codigo,
    ILogger<SQLiteService> logger,
    IProductoDatabaseService productoDatabaseService,
    IInventarioDatabaseService inventarioDatabaseService)
    {
        _productoDatabaseService = productoDatabaseService;
        _inventarioDatabaseService = inventarioDatabaseService;
        codigoOriginal = codigo;
        InitializeComponents();
        CargarDatosProducto().Wait();
        ConfigureEvents();
    }

    private void InitializeComponents()
    {
        this.Text = "Modificar Producto";
        this.Size = new Size(400, 300);

        // Crear y configurar los controles
        Label codigoLabel = new Label { Text = "Código:", Location = new Point(20, 20) };
        codigoTextBox = new TextBox { Location = new Point(120, 20), Width = 200 };

        Label descripcionLabel = new Label { Text = "Descripción:", Location = new Point(20, 50) };
        descripcionTextBox = new TextBox { Location = new Point(120, 50), Width = 200 };

        Label kilosLabel = new Label { Text = "Kilos:", Location = new Point(20, 80) };
        kilosTextBox = new TextBox { Location = new Point(120, 80), Width = 200 };

        Label unidadesLabel = new Label { Text = "Unidades:", Location = new Point(20, 110) };
        unidadesTextBox = new TextBox { Location = new Point(120, 110), Width = 200 };

        Label fechaLabel = new Label { Text = "Vencimiento:", Location = new Point(20, 140) };
        fechaVencimientoPicker = new DateTimePicker { Location = new Point(120, 140), Width = 200 };

        guardarButton = new Button
        {
            Text = "Guardar",
            Location = new Point(120, 180),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White
        };

        cancelarButton = new Button
        {
            Text = "Cancelar",
            Location = new Point(230, 180),
            Size = new Size(100, 30)
        };

        this.Controls.AddRange(new Control[] {
            codigoLabel, codigoTextBox,
            descripcionLabel, descripcionTextBox,
            kilosLabel, kilosTextBox,
            unidadesLabel, unidadesTextBox,
            fechaLabel, fechaVencimientoPicker,
            guardarButton, cancelarButton
        });
    }

    private async Task CargarDatosProducto()
    {
        try
        {
            var producto = await _productoDatabaseService.GetByCodigo(codigoOriginal);
            if (producto != null)
            {
                codigoTextBox.Text = producto.Codigo;
                descripcionTextBox.Text = producto.Nombre;
                kilosTextBox.Text = producto.Kilos.ToString();
                unidadesTextBox.Text = producto.Unidades.ToString();
                if (DateTime.TryParse(producto.FechaMasNueva, out DateTime fecha))
                {
                    fechaVencimientoPicker.Value = fecha;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar los datos del producto: {ex.Message}");
        }
    }

    private void ConfigureEvents()
    {
        guardarButton.Click += GuardarButton_Click;
        cancelarButton.Click += (s, e) => this.Close();
    }
    private readonly IProductoDatabaseService _productoDatabaseService;
    private readonly IInventarioDatabaseService _inventarioDatabaseService;

    private async void GuardarButton_Click(object sender, EventArgs e)
    {
        try
        {
            var producto = new Producto
            {
                Codigo = codigoTextBox.Text,
                Nombre = descripcionTextBox.Text,
                Kilos = double.Parse(kilosTextBox.Text),
                Unidades = int.Parse(unidadesTextBox.Text)
            };

            await _productoDatabaseService.UpdateAsync(producto);
            await _inventarioDatabaseService.ActualizarFechasInventarioAsync(
                producto.Codigo,
                fechaVencimientoPicker.Value);

            MessageBox.Show("Producto actualizado exitosamente", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al actualizar el producto: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
