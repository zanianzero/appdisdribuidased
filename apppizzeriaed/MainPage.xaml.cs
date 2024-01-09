using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace apppizzeriaed
{
    public partial class MainPage : ContentPage
    {

        string apiUrl = "https://appmovilesapipizza.azurewebsites.net/";


        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            lblTexto.Text = txtDato.Text;
        }

        private void txtDato_TextChanged(System.Object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            lblTexto.Text = txtDato.Text;
        }

        private async void Button_Clicked_1(Object sender, EventArgs e)
        {
            // LISTAR TODOS LOS NOMBRES DE LOS RESTAURANTES ASI SEAN MAS DE 1
            using (var webClient = new HttpClient())
            {
                try
                {
                    // Asíncronamente espera a que la tarea termine
                    var json = await webClient.GetStringAsync(apiUrl + "api/Restaurantes");
                    var restaurantes = Newtonsoft.Json.JsonConvert.DeserializeObject<Restaurante[]>(json);
                    // Actualiza la UI en el hilo principal
                    Device.BeginInvokeOnMainThread(() => {
                        // Muestra el ID y el Nombre de los restaurantes
                        listaRestaurantes.ItemsSource = restaurantes.Select(r => $"{r.CodigoRestaurante+"   "} {r.Nombre}").ToList();
                        txtDato.Text = string.Empty;
                        txtNombre.Text = string.Empty;
                        txtDireccion.Text = string.Empty;
                        txtTelefono.Text = string.Empty;
                        txtEspecialidad.Text = string.Empty;
                    });
                }
                catch (Exception ex)
                {
                    // Maneja cualquier error que pueda ocurrir
                    Debug.WriteLine($"Error al obtener restaurantes: {ex.Message}");
                    Device.BeginInvokeOnMainThread(() => {
                        lblTexto.Text = "Error al cargar los datos.";
                    });
                }
            }
        }

        private async void Button_Clicked_2(Object sender, EventArgs e)
        {
            // Primero, verifica si el campo del ID está vacío.
            if (string.IsNullOrWhiteSpace(txtDato.Text))
            {
                await DisplayAlert("Error", "Por favor, ingresa el ID del restaurante.", "OK");
                return; // Salir del método si no hay ID.
            }

            try
            {
                using (var webClient = new HttpClient())
                {
                    var json = await webClient.GetStringAsync(apiUrl + "api/Restaurantes/" + txtDato.Text);
                    var restaurante = Newtonsoft.Json.JsonConvert.DeserializeObject<Restaurante>(json);

                    // Si la deserialización fue exitosa y el restaurante no es nulo, actualiza los campos de texto.
                    if (restaurante != null)
                    {
                        txtNombre.Text = restaurante.Nombre;
                        txtDireccion.Text = restaurante.Direccion;
                        txtTelefono.Text = restaurante.Telefono;
                        txtEspecialidad.Text = restaurante.Especialidad;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Restaurante no encontrado.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                // Si hay una excepción, muestra un mensaje genérico al usuario.
                await DisplayAlert("Error", "No se pudo cargar la información del restaurante. Asegúrate de que el ID sea correcto.", "OK");
                Debug.WriteLine(ex.Message); // Solo para propósitos de desarrollo.
            }
        }


        // buscar restaurantes por id api/Restaurantes/(+ el codigo del producto)

        private async void cmdReadOne_Clicked(Object sender, EventArgs e)
        {
            try
            {
                using (var webClient = new HttpClient())
                {
                    var json = await webClient.GetStringAsync(apiUrl + "api/Restaurantes/" + txtDato.Text);
                    var restaurante = Newtonsoft.Json.JsonConvert.DeserializeObject<Restaurante>(json);

                    // Crear una lista con los detalles del restaurante.
                    var detallesRestaurante = new List<string>
            {
                $"Nombre: {restaurante.Nombre}",
                $"Dirección: {restaurante.Direccion}",
                $"Teléfono: {restaurante.Telefono}",
                $"Especialidad: {restaurante.Especialidad}"
            };

                    // Asignar la lista al ItemsSource del ListView.
                    listaRestaurantes.ItemsSource = detallesRestaurante;
                    txtDato.Text = string.Empty;
                    txtNombre.Text = string.Empty;
                    txtDireccion.Text = string.Empty;
                    txtTelefono.Text = string.Empty;
                    txtEspecialidad.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                lblTexto.Text = "No hay datos con ese código o error en la búsqueda.";
                // En el desarrollo, podrías querer imprimir el mensaje de error completo.
                Console.WriteLine(ex.Message);
            }
        }


        // insertar restaurantes api/Restaurantes
        private void cmdInsert_Clicked(Object sender, EventArgs e)
        {
            // Verifica que todos los campos están llenos
            if (!string.IsNullOrWhiteSpace(txtNombre.Text) &&
                !string.IsNullOrWhiteSpace(txtDireccion.Text) &&
                !string.IsNullOrWhiteSpace(txtTelefono.Text) &&
                !string.IsNullOrWhiteSpace(txtEspecialidad.Text))
            {

                using (var webClient = new HttpClient())
                {
                    webClient.BaseAddress = new Uri(apiUrl+"api/Restaurantes");
                    webClient.DefaultRequestHeaders.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(new Restaurante
                    {
                        CodigoRestaurante = 0,
                        Nombre = txtNombre.Text,
                        Direccion = txtDireccion.Text,
                        Telefono = txtTelefono.Text,
                        Especialidad = txtEspecialidad.Text
                    });
                    var request = new HttpRequestMessage(HttpMethod.Post, apiUrl+"api/Restaurantes");
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp = webClient.SendAsync(request);
                    resp.Wait();
                    json = resp.Result.Content.ReadAsStringAsync().Result;
                    var restaurante = JsonConvert.DeserializeObject<Restaurante>(json);
                    Button_Clicked_1(this, new EventArgs());
                    restaurante.CodigoRestaurante.ToString();
                    txtDato.Text = string.Empty;
                    txtNombre.Text = string.Empty;
                    txtDireccion.Text = string.Empty;
                    txtTelefono.Text = string.Empty;
                    txtEspecialidad.Text = string.Empty;
                }

            }
            else
            {
                // No todos los campos están llenos, mostrar un mensaje al usuario
                DisplayAlert("Error", "Por favor, completa todos los campos.", "OK");
            }

        }


        private async void cmdUpdate_Clicked(Object sender, EventArgs e)
        {
            // Primero, verifica si el campo del ID está vacío.
            if (string.IsNullOrWhiteSpace(txtDato.Text))
            {
                await DisplayAlert("Error", "Por favor, ingresa el ID del restaurante.", "OK");
                return; // Salir del método si no hay ID.
            }

            // Luego, verifica si los otros campos están llenos.
            if (!string.IsNullOrWhiteSpace(txtNombre.Text) &&
                !string.IsNullOrWhiteSpace(txtDireccion.Text) &&
                !string.IsNullOrWhiteSpace(txtTelefono.Text) &&
                !string.IsNullOrWhiteSpace(txtEspecialidad.Text))
            {
                // Intenta convertir el ID a un número entero.
                if (!int.TryParse(txtDato.Text, out int codigoRestaurante))
                {
                    await DisplayAlert("Error", "El ID proporcionado no es válido.", "OK");
                    return; // Salir del método si el ID no es un número.
                }

                var restaurante = new Restaurante
                {
                    CodigoRestaurante = codigoRestaurante,
                    Nombre = txtNombre.Text,
                    Direccion = txtDireccion.Text,
                    Telefono = txtTelefono.Text,
                    Especialidad = txtEspecialidad.Text
                };

                var json = JsonConvert.SerializeObject(restaurante);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var webClient = new HttpClient())
                {
                    var response = await webClient.PutAsync(apiUrl + "api/Restaurantes/" + txtDato.Text, content);
                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "Restaurante actualizado correctamente.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Error al actualizar el restaurante.", "OK");
                    }
                }
                // Limpiar los campos después de actualizar
                txtDato.Text = string.Empty;
                txtNombre.Text = string.Empty;
                txtDireccion.Text = string.Empty;
                txtTelefono.Text = string.Empty;
                txtEspecialidad.Text = string.Empty;
            }
            else
            {
                // Si no todos los campos requeridos están llenos, muestra un mensaje de error.
                await DisplayAlert("Error", "Por favor, completa todos los campos.", "OK");
            }
        }






        private async void cmdDelete_Clicked(Object sender, EventArgs e)
        {
            // Verifica que se haya ingresado un ID.
            if (string.IsNullOrWhiteSpace(txtDato.Text))
            {
                await DisplayAlert("Error", "Por favor, ingresa el ID del restaurante a eliminar.", "OK");
                return;
            }

            // Pregunta al usuario si está seguro de querer eliminar el restaurante.
            bool confirm = await DisplayAlert("Confirmar", "¿Estás seguro de que quieres eliminar el restaurante?", "Sí", "No");
            if (!confirm)
            {
                return; // Si el usuario no confirma, cancela la operación.
            }

            try
            {
                using (var webClient = new HttpClient())
                {
                    // Envía la solicitud DELETE a la API.
                    var response = await webClient.DeleteAsync(apiUrl + "api/Restaurantes/" + txtDato.Text);

                    if (response.IsSuccessStatusCode)
                    {
                        // Si la API responde con éxito, muestra un mensaje de confirmación.
                        await DisplayAlert("Éxito", "Restaurante eliminado correctamente.", "OK");
                        Button_Clicked_1(this, new EventArgs());
                    }
                    else
                    {
                        // Si hay un problema con la solicitud, muestra un mensaje de error.
                        await DisplayAlert("Error", "Error al eliminar el restaurante.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                // En caso de una excepción, muestra un mensaje de error.
                await DisplayAlert("Error", "Ocurrió un error al intentar eliminar el restaurante.", "OK");
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                // Limpia el campo del ID después de la operación.
                txtDato.Text = string.Empty;
            }
        }




























    }


}

    










