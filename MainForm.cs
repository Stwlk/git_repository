using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp
{
    public partial class MainForm : Form
    {
        private static readonly HttpClient client = new HttpClient();

        public MainForm()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://localhost:5000/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async void btnGetItems_Click(object sender, EventArgs e)
        {
            await GetItemsAsync();
        }

        private async Task GetItemsAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("items");
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<List<Item>>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                listBoxItems.Items.Clear();
                foreach (var item in items)
                {
                    listBoxItems.Items.Add($"{item.Id}: {item.Name} - {item.Description}");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"JSON error: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}");
            }
        }

        private async void btnAddItem_Click(object sender, EventArgs e)
        {
            var newItem = new Item
            {
                Name = txtName.Text,
                Description = txtDescription.Text
            };
            await AddItemAsync(newItem);
        }

        private async Task AddItemAsync(Item item)
        {
            var json = JsonSerializer.Serialize(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("items", content);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Item added successfully");
                await GetItemsAsync();  // Refresh the list after adding
            }
            else
            {
                MessageBox.Show("Failed to add item.");
            }
        }

        private async void btnUpdateItem_Click(object sender, EventArgs e)
        {
            var itemId = int.Parse(txtId.Text);
            var updatedItem = new Item
            {
                Id = itemId,
                Name = txtName.Text,
                Description = txtDescription.Text
            };
            await UpdateItemAsync(itemId, updatedItem);
        }

        private async Task UpdateItemAsync(int id, Item item)
        {
            var json = JsonSerializer.Serialize(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync($"items/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Item updated successfully");
                await GetItemsAsync();  // Refresh the list after updating
            }
            else
            {
                MessageBox.Show("Failed to update item.");
            }
        }

        private async void btnDeleteItem_Click(object sender, EventArgs e)
        {
            var itemId = int.Parse(txtId.Text);
            await DeleteItemAsync(itemId);
        }

        private async Task DeleteItemAsync(int id)
        {
            HttpResponseMessage response = await client.DeleteAsync($"items/{id}");
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Item deleted successfully");
                await GetItemsAsync();  // Refresh the list after deleting
            }
            else
            {
                MessageBox.Show("Failed to delete item.");
            }
        }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
