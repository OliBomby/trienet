// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using System.Diagnostics;
using TrieNet;
using TrieNet.Ukkonen;

namespace DemoApp;

public partial class MainForm : Form {
    private static readonly char[] Delimiters = { ' ', '\r', '\n' };
    private readonly UkkonenTrie<char, string> trie;
    private long wordCount;

    public MainForm() {
        InitializeComponent();
        trie = new UkkonenTrie<char, string>(3);
        folderName.Text =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "texts");
    }

    private void LoadFile(string fileName) {
        var word = File.ReadAllText(fileName);
        trie.Add(word.AsMemory(), Path.GetFileName(fileName));
        wordCount += word.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
        Debug.WriteLine($"Loaded {word.Length} characters.");
        Debug.WriteLine($"Trie size = {trie.Size}");
    }

    private void UpdateProgress(int position) {
        progressBar1.Value = Math.Min(position, progressBar1.Maximum);
        Application.DoEvents();
    }

    private void textBox1_TextChanged(object sender, EventArgs e) {
        var text = textBox1.Text;
        if (string.IsNullOrEmpty(text) || text.Length < 3) return;
        var result = trie.RetrieveSubstrings(text.AsSpan()).ToArray();
        listBox1.Items.Clear();
        foreach (var wordPosition in result) listBox1.Items.Add(wordPosition);
    }

    private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
        var item = (WordPosition<string>)listBox1.SelectedItem;
        var word = File.ReadAllText(Path.Combine("texts", item.Value));
        const int bifferSize = 300;
        var position = Math.Max(item.CharPosition - bifferSize / 2, 0);
        var line = word.Substring(position, bifferSize);
        richTextBox1.Text = line;

        var serachText = textBox1.Text;
        var index = richTextBox1.Text.IndexOf(serachText, StringComparison.InvariantCultureIgnoreCase);
        if (index < 0) return;
        richTextBox1.Select(index, serachText.Length);
        richTextBox1.SelectionBackColor = Color.Yellow;
        richTextBox1.DeselectAll();
    }

    private void buttonBrowse_Click(object sender, EventArgs e) {
        var folderBrowserDialog = new FolderBrowserDialog();
        folderBrowserDialog.SelectedPath = folderName.Text;
        var result = folderBrowserDialog.ShowDialog();
        if (result != DialogResult.OK) return;
        folderName.Text = folderBrowserDialog.SelectedPath;
    }

    private void btnLoad_Click(object sender, EventArgs e) {
        LoadAll();
    }

    private void LoadAll() {
        wordCount = 0;
        var path = folderName.Text;
        if (!Directory.Exists(path)) return;
        var files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
        progressBar1.Minimum = 0;
        progressBar1.Maximum = files.Length;
        progressBar1.Step = 1;
        for (var index = 0; index < files.Length; index++) {
            var file = files[index];
            progressText.Text = $@"Processing file {index + 1} of {files.Length}: [{Path.GetFileName(file)}]";
            Application.DoEvents();

            LoadFile(file);
            UpdateProgress(index + 1);
        }

        progressText.Text = $@"{wordCount:n0} words read. Ready.";
        UpdateProgress(0);
    }
}