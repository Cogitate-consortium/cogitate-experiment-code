
% 1 such function file for each plot.
% Each of these plot functions are called from a function called plot_durations which is called from
% ECoGTest twice. Once for each file and then once for all files.
function ECoG_hists_and_stats_durations(StimDur, StimDurAccuracy, JitterDur, JitterDurAccuracy, StimDurAccuracy_center, StimDurAccuracy_right, StimDurAccuracy_left, StimDurAccuracy_face, StimDurAccuracy_object, StimDurAccuracy_letter, StimDurAccuracy_false, StimDurAccuracy_05, StimDurAccuracy_10, StimDurAccuracy_15, StimDurAccuracy_target, StimDurAccuracy_non_target, StimDurAccuracy_irrelevant, time_stamp_type, plot_suffix)
    
    global save_plots font_size VERBOSE show_only_plots_that_are_not_saved
    
    if VERBOSE
        disp('Inside ECoG_hists_and_stats_durations with ')
        plot_suffix
    end


    %%%%%%%%% plotting
    
    %% Durations stimuli %%
    if show_only_plots_that_are_not_saved
        figure('visible', 'off');
    else
        figure();
    end
    title_str = sprintf('Durations of stimuli as measured from the %s [ms]', time_stamp_type );
    title(title_str, 'FontSize', font_size);
    xlabel('Durations [ms]', 'FontSize', font_size);
    ylabel('Frequency', 'FontSize', font_size);
    hold on;
    histogram(StimDur, 100);
    if save_plots
        saveas(gcf, sprintf('Durations_stimuli_from_%s_%s.png', time_stamp_type, plot_suffix));
    end
    
     %% Durations jitters %%
    if show_only_plots_that_are_not_saved
        figure('visible', 'off');
    else
        figure();
    end
    histogram(JitterDur);
    ylabel('Number of trials', 'FontSize', font_size);
    xlabel('Measured Jitter duration [s]', 'FontSize', font_size);
    title_txt = sprintf('Durations of jitters as measured by the %s [s]', time_stamp_type );
    title(title_txt,'FontSize', font_size);
    if save_plots
        saveas(gcf, sprintf('Durations_jitters_from_%s_%s.png', time_stamp_type, plot_suffix));
    end
    
    %% Stats stimuli duration accuracies for different cat, rel, dur and oris
    
        %%%%%%%%%%%% Check significance with anova

    % Orientation

    sc = size(StimDurAccuracy_center,1);
    sl = size(StimDurAccuracy_left,1);
    sr = size(StimDurAccuracy_right,1);


    groups_ori = {};
    for i = 1:sc
        groups_ori{i} = 'center';
    end

    for i = (1 + sc):(sc + sl)
        groups_ori{i} = 'left';
    end

    for i = (1+ sc + sl):(sc + sl + sr)
        groups_ori{i} = 'right';
    end

    data_vector_ori = [StimDurAccuracy_center' StimDurAccuracy_right' StimDurAccuracy_left'];
    anova_test_ori = anova1(data_vector_ori, groups_ori, 'off' );


    %anova_test

    % Category

    sf = size(StimDurAccuracy_face,1);
    so = size(StimDurAccuracy_object,1);
    sl = size(StimDurAccuracy_letter,1);
    sfalse = size(StimDurAccuracy_false,1);


    groups_cat = {};
    for i = 1:sf
        groups_cat{i} = 'face';
    end

    for i = (1 + sf):(sf + so)
        groups_cat{i} = 'object';
    end

    for i = (1+ sf + so):(sf + so + sl)
        groups_cat{i} = 'letter';
    end

    for i = (1+ sf + so + sl):(sf + so+ sl + sfalse)
        groups_cat{i} = 'false';
    end

    data_vector_cat = [StimDurAccuracy_face' StimDurAccuracy_object' StimDurAccuracy_letter' StimDurAccuracy_false'];
    % If you want to see the anova plots, you can remove the 'off'
    % parameter
    anova_test_cat = anova1(data_vector_cat, groups_cat, 'off' );
    

    % Relevance

    st = size(StimDurAccuracy_target,1);
    sn = size(StimDurAccuracy_non_target,1);
    si = size(StimDurAccuracy_irrelevant,1);

    groups_rel = {};
    for i = 1:st
        groups_rel{i} = 'target';
    end

    for i = (1 + st):(st + sn)
        groups_rel{i} = 'non_target';
    end

    for i = (1+ st + sn):(st + sn + si)
        groups_rel{i} = 'irrelevant';
    end

    data_vector_rel = [StimDurAccuracy_target' StimDurAccuracy_non_target' StimDurAccuracy_irrelevant'];
    anova_test_rel = anova1(data_vector_rel, groups_rel, 'off' );


    % Duration

    s05 = size(StimDurAccuracy_05,1);
    s10 = size(StimDurAccuracy_10,1);
    s15 = size(StimDurAccuracy_15,1);

    groups_dur = {};
    for i = 1:s05
        groups_dur{i} = '0.5';
    end

    for i = (1 + s05):(s05 + s10)
        groups_dur{i} = '1.0';
    end

    for i = (1+ s05 + s10):(s05 + s10 + s15)
        groups_dur{i} = '1.5';
    end

    data_vector_dur = [StimDurAccuracy_05' StimDurAccuracy_10' StimDurAccuracy_15'];
    anova_test_dur = anova1(data_vector_dur, groups_dur, 'off');
    
    %% Plots %%

    color1 = '#000000';
    color2 = '#ACDBE8';
    color3 = '#FFFAA7';
    color4 = '#FFD3A7';

    % In order to get the bins the same between all plots, we should set the
    % range on beforehand
    x_max = max(StimDurAccuracy);
    x_min = min(StimDurAccuracy);
    n_bins = 50;
  
    edges = x_min: (x_max-x_min)/n_bins: x_max;
    
  

    % Orientation
    if show_only_plots_that_are_not_saved
        figure('visible', 'off');
    else
        figure();
    end
    title_str = sprintf('Difference between stimulus durations [ms] \n as measured by %s, and as planned \n for different stimuli orientations', time_stamp_type );
    title(title_str, 'FontSize', font_size);
    xlabel('Difference [ms]', 'FontSize', font_size);
    ylabel('Frequency', 'FontSize', font_size);
    xlim([x_min, x_max]);
    hold on;
    h_c = histogram(StimDurAccuracy_center, edges,  'FaceColor', color1);
    hold on
    h_r =histogram(StimDurAccuracy_right, edges,  'FaceColor', color2);
    hold on
    h_l = histogram(StimDurAccuracy_left, edges, 'FaceColor', color3);
    
    % Add info about ANOVA
    txt_anova = sprintf('ANOVA p-value: %s', num2str(anova_test_ori,3));
    posx1 = 0.5*(x_min + (x_max - x_min));
    posy1 =  0.5*max( [max(h_c.Values), max(h_r.Values), max(h_l.Values)] );
    text( posx1, posy1, txt_anova, 'FontSize', font_size);
    
    % Add info about mean and standard deviation:
    legend_center = sprintf('%s %s %s %s %s', 'Center (avg = ', num2str(mean(StimDurAccuracy_center),3), ', std = ', num2str(std(StimDurAccuracy_center),3), ')' );
    legend_right = sprintf('%s %s %s %s %s', 'Right (avg = ', num2str(mean(StimDurAccuracy_right),3), ', std = ', num2str(std(StimDurAccuracy_right),3), ')' );
    legend_left = sprintf('%s %s %s %s %s', 'Left (avg = ', num2str(mean(StimDurAccuracy_left),3), ', std = ', num2str(std(StimDurAccuracy_left),3), ')' );

    legend(legend_center, legend_right, legend_left, 'FontSize', font_size);
    if save_plots
        saveas(gcf, sprintf('Duration_accuracy_stimuli_from_%s_orientation_%s.png', time_stamp_type, plot_suffix));
    end

    % Category

    if show_only_plots_that_are_not_saved
        figure('visible', 'off');
    else
        figure();
    end
    title_str = sprintf('Difference between stimulus durations [ms] \n as measured by %s, and as planned \n for different stimuli categories', time_stamp_type );
    title(title_str, 'FontSize', font_size);
    xlabel('Difference [ms]', 'FontSize', font_size);
    ylabel('Frequency', 'FontSize', font_size);
    xlim([x_min, x_max]);
    hold on;
    h_f = histogram(StimDurAccuracy_face,edges,  'FaceColor', color1);
    hold on
    h_o = histogram(StimDurAccuracy_object,edges,  'FaceColor', color2);
    hold on
    h_l = histogram(StimDurAccuracy_letter,edges, 'FaceColor', color3);
    hold on
    h_ff = histogram(StimDurAccuracy_false,edges, 'FaceColor', color4);
    
    % Add info about ANOVA
    txt_anova = sprintf('ANOVA p-value: %s', num2str(anova_test_cat,3));
    posx1 = 0.5*(x_min + (x_max - x_min));
    posy1 =  0.5*max( [max(h_f.Values), max(h_o.Values), max(h_l.Values), max(h_ff.Values)] );
    text( posx1, posy1, txt_anova, 'FontSize', font_size);

    % Add info about mean and standard deviation:
    legend_face = sprintf('%s %s %s %s %s', 'Face (avg = ', num2str(mean(StimDurAccuracy_face),3), ', std = ', num2str(std(StimDurAccuracy_face),3), ')' );
    legend_object = sprintf('%s %s %s %s %s', 'Object (avg = ', num2str(mean(StimDurAccuracy_object),3), ', std = ', num2str(std(StimDurAccuracy_object),3), ')' );
    legend_letter = sprintf('%s %s %s %s %s', 'Letter (avg = ', num2str(mean(StimDurAccuracy_letter),3), ', std = ', num2str(std(StimDurAccuracy_letter),3), ')' );
    legend_false = sprintf('%s %s %s %s %s', 'False (avg = ', num2str(mean(StimDurAccuracy_false),3), ', std = ', num2str(std(StimDurAccuracy_false),3), ')' );

    legend(legend_face, legend_object, legend_letter, legend_false, 'FontSize', font_size);
    
     if save_plots
        saveas(gcf, sprintf('Duration_accuracy_stimuli_from_%s_category_%s.png', time_stamp_type, plot_suffix));
     end
    
    % Relevance

    if show_only_plots_that_are_not_saved
        figure('visible', 'off');
    else
        figure();
    end
    title_str = sprintf('Difference between stimulus durations [ms] \n as measured by %s, and as planned \n for different stimuli relevances', time_stamp_type );
    title(title_str, 'FontSize', font_size);
    xlabel('Difference [ms]', 'FontSize', font_size);
    ylabel('Frequency', 'FontSize', font_size);
    xlim([x_min, x_max]);
    hold on;
    h_t = histogram(StimDurAccuracy_target,edges,  'FaceColor', color1);
    hold on
    h_nt = histogram(StimDurAccuracy_non_target,edges,  'FaceColor', color2);
    hold on
    h_i = histogram(StimDurAccuracy_irrelevant,edges, 'FaceColor', color3);
    
    % Add info about ANOVA
    txt_anova = sprintf('ANOVA p-value: %s', num2str(anova_test_rel,3));
    posx1 = 0.5*(x_min + (x_max - x_min));
    posy1 =  0.5*max( [max(h_t.Values), max(h_nt.Values), max(h_i.Values)] );
    text( posx1, posy1, txt_anova, 'FontSize', font_size);

    % Add info about mean and standard deviation:
    legend_target = sprintf('%s %s %s %s %s', 'target (avg = ', num2str(mean(StimDurAccuracy_target),3), ', std = ', num2str(std(StimDurAccuracy_target),3), ')' );
    legend_irrelevant = sprintf('%s %s %s %s %s', 'irrelevant (avg = ', num2str(mean(StimDurAccuracy_irrelevant),3), ', std = ', num2str(std(StimDurAccuracy_irrelevant),3), ')' );
    legend_non_target = sprintf('%s %s %s %s %s', 'non-target (avg = ', num2str(mean(StimDurAccuracy_non_target),3), ', std = ', num2str(std(StimDurAccuracy_non_target),3), ')' );

    legend( legend_target, legend_irrelevant, legend_non_target, 'FontSize', font_size);
    
    if save_plots
        saveas(gcf, sprintf('Duration_accuracy_stimuli_from_%s_relevance_%s.png', time_stamp_type, plot_suffix));
    end


    % Duration

    if show_only_plots_that_are_not_saved
        figure('visible', 'off');
    else
        figure();
    end
    title_str = sprintf('Difference between stimulus durations [ms] \n as measured by %s, and as planned \n for different stimuli durations', time_stamp_type );
    title(title_str, 'FontSize', font_size);
    xlabel('Difference [ms]', 'FontSize', font_size);
    ylabel('Frequency', 'FontSize', font_size);
    xlim([x_min, x_max]);
    hold on;
    h_05 = histogram(StimDurAccuracy_05,edges,  'FaceColor', color1);
    hold on
    h_10 = histogram(StimDurAccuracy_10,edges, 'FaceColor', color3);
    hold on
    h_15 = histogram(StimDurAccuracy_15,edges,  'FaceColor', color2);
    
    % Add info about ANOVA
    txt_anova = sprintf('ANOVA p-value: %s', num2str(anova_test_dur,3));
    posx1 = 0.5*(x_min + (x_max - x_min));
    posy1 =  0.5*max( [max(h_05.Values), max(h_10.Values), max(h_15.Values)] );
    text( posx1, posy1, txt_anova, 'FontSize', font_size);
    
    % Add info about mean and standard deviation:
    legend_05 = sprintf('%s %s %s %s %s', '0.5 s (avg = ', num2str(mean(StimDurAccuracy_05),3), ', std = ', num2str(std(StimDurAccuracy_05),3), ')' );
    legend_10 = sprintf('%s %s %s %s %s', '1.0 s (avg = ', num2str(mean(StimDurAccuracy_10),3), ', std = ', num2str(std(StimDurAccuracy_10),3), ')' );
    legend_15 = sprintf('%s %s %s %s %s', '1.5 s (avg = ', num2str(mean(StimDurAccuracy_15),3), ', std = ', num2str(std(StimDurAccuracy_15),3), ')' );

    legend(legend_05, legend_10, legend_15, 'FontSize', font_size);
    if save_plots
        saveas(gcf, sprintf('Duration_accuracy_stimuli_from_%s_duration_%s.png', time_stamp_type, plot_suffix));
    end
    




end